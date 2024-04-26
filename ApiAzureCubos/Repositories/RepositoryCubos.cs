using ApiAzureCubos.Data;
using ApiAzureCubos.Models;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;

namespace ApiAzureCubos.Repositories
{
    public class RepositoryCubos
    {
        private CubosContext context;
        private SecretClient secretClient;
        private string BlobCubos;

        public RepositoryCubos(CubosContext context, SecretClient secretClient)
        {
            this.context = context;
            this.secretClient = secretClient;
            Task.Run(async () =>
            {
                KeyVaultSecret secretBlob = await this.secretClient.GetSecretAsync("BlobCubos");
                this.BlobCubos = secretBlob.Value;
            });
        }

        public async Task<List<Cubo>> GetCubosAsync()
        {
            List<Cubo> cubos = await this.context.Cubos.ToListAsync();
            cubos.ForEach(c => c.Imagen = this.BlobCubos + c.Imagen);
            return cubos;
        }

        public async Task<List<Cubo>> GetCubosMarcaAsync(string marca)
        {
            List<Cubo> cubos = await this.context.Cubos
                .Where(c => c.Marca == marca)
                .ToListAsync();
            cubos.ForEach(c => c.Imagen = this.BlobCubos + c.Imagen);
            return cubos;
        }

        public async Task CreateUsuarioAsync(UsuarioCubo usuario)
        {
            if (await this.context.UsuariosCubo.CountAsync() == 0)
                usuario.IdUsuario = 1;
            else
                usuario.IdUsuario = await this.context.UsuariosCubo.MaxAsync(u => u.IdUsuario) + 1;
            await this.context.UsuariosCubo.AddAsync(usuario);
            await this.context.SaveChangesAsync();
        }

        public async Task<UsuarioCubo> LoginUsuarioAsync(LoginModel model)
        {
            return await this.context.UsuariosCubo
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Pass == model.Pass);
        }

        public async Task<List<CompraCubo>> GetComprasUsuarioAsync(int idusuario)
        {
            return await this.context.ComprasCubo
                .Where(c => c.IdUsuario == idusuario)
                .ToListAsync();
        }

        public async Task CreateCompraCubo(int idusuario, int idcubo)
        {
            int idcompra;
            if (await this.context.ComprasCubo.CountAsync() == 0)
                idcompra = 1;
            else
                idcompra = await this.context.ComprasCubo.MaxAsync(c => c.IdPedido) + 1;
            CompraCubo compra = new CompraCubo
            {
                IdPedido = idcompra,
                IdCubo = idcubo,
                IdUsuario = idusuario,
                FechaPedido = DateTime.UtcNow
            };
            await this.context.ComprasCubo.AddAsync(compra);
            await this.context.SaveChangesAsync();
        }
    }
}
