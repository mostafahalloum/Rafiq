using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rafiq.Models;

public class MedicineActiveIngredientsConfiguration
    : IEntityTypeConfiguration<MedicineActiveIngredients>
{
    public void Configure ( EntityTypeBuilder<MedicineActiveIngredients> builder )
    {
        builder.HasKey(ma => new { ma.MedicineId, ma.ActiveIngredientsId });

        builder.HasOne(ma => ma.medicine)
               .WithMany(m => m.MedicineActiveIngredients)
               .HasForeignKey(ma => ma.MedicineId);

 
        builder.HasOne(ma => ma.activeIngredients)
               .WithMany(a => a.MedicineActiveIngredients)
               .HasForeignKey(ma => ma.ActiveIngredientsId);
    }
}