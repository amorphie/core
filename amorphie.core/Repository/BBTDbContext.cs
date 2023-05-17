using amorphie.core.Base;
using amorphie.core.Identity;
using Microsoft.EntityFrameworkCore;

namespace amorphie.core.Repository
{
    public abstract class BBTDbContext : DbContext
    {
        internal IBBTIdentity bbtIdentity;
        public BBTDbContext(DbContextOptions options, IBBTIdentity _bbtIdentity) : base(options)
        {
            bbtIdentity = _bbtIdentity;
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = bbtIdentity.UserId.Value;
                        entry.Entity.CreatedByBehalfOf = bbtIdentity.BehalfOfId.Value;

                        entry.Entity.ModifiedAt = entry.Entity.CreatedAt;
                        entry.Entity.ModifiedBy = entry.Entity.CreatedBy;
                        entry.Entity.ModifiedByBehalfOf = entry.Entity.CreatedByBehalfOf;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = DateTime.UtcNow;
                        entry.Entity.ModifiedBy = bbtIdentity.UserId.Value;
                        entry.Entity.ModifiedByBehalfOf = bbtIdentity.BehalfOfId.Value;
                        break;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }
    }
}
