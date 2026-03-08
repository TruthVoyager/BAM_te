using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("Rank")]
    public class Rank
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
    }

    public class RankConfiguration : IEntityTypeConfiguration<Rank>
    {
        public void Configure(EntityTypeBuilder<Rank> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasIndex(x => x.Level).IsUnique();
        }
    }
}
