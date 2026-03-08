using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StargateAPI.Business.Data
{
    [Table("ProcessLog")]
    public class ProcessLog
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string Level { get; set; } = string.Empty; // "Success" or "Error"
        public string Message { get; set; } = string.Empty;
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public int? StatusCode { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionStackTrace { get; set; }
    }

    public class ProcessLogConfiguration : IEntityTypeConfiguration<ProcessLog>
    {
        public void Configure(EntityTypeBuilder<ProcessLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Level).HasMaxLength(20);
            builder.Property(x => x.Message).HasMaxLength(2000);
            builder.Property(x => x.RequestPath).HasMaxLength(500);
            builder.Property(x => x.RequestMethod).HasMaxLength(10);
            builder.Property(x => x.ExceptionMessage).HasMaxLength(4000);
            builder.Property(x => x.ExceptionStackTrace).HasMaxLength(8000);
        }
    }
}
