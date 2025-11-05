using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enums
{
    /// <summary>
    /// Типы химических производств
    /// </summary>
    public enum ChemicalIndustryType
    {
        // Основная химия
        PetrochemicalPlant,
        FertilizerPlant,
        PolymerPlant,
        AcidProduction,
        BaseChemicals,

        // Специализированная химия
        PharmaceuticalPlant,
        PaintAndVarnish,
        CosmeticsChemical,
        Agrochemicals,
        CleaningChemicals,

        // Высокотехнологичная химия
        FineChemicals,
        SpecialtyChemicals,
        Biotechnology,
        Nanomaterials
    }

    /// <summary>
    /// Типы химического сырья
    /// </summary>
    public enum ChemicalMaterial
    {
        // Основное сырье
        CrudeOil,
        NaturalGas,
        Coal,
        Salt,
        Sulfur,
        Phosphates,
        Limestone,

        // Химические реагенты
        Ammonia,
        Chlorine,
        Hydrogen,
        Oxygen,
        Nitrogen,
        SulfuricAcid,
        NitricAcid,

        // Органические соединения
        Ethylene,
        Propylene,
        Benzene,
        Methanol,
        Ethanol
    }

    /// <summary>
    /// Типы химической продукции
    /// </summary>
    public enum ChemicalProduct
    {
        // Основные химикаты
        Ammonia,
        Urea,
        SulfuricAcid,
        NitricAcid,
        CausticSoda,
        SodaAsh,

        // Удобрения
        NitrogenFertilizers,
        PhosphorusFertilizers,
        PotassiumFertilizers,
        ComplexFertilizers,

        // Полимеры и пластмассы
        Polyethylene,
        Polypropylene,
        PVC,
        Polystyrene,
        SyntheticRubber,

        // Специальные химикаты
        Pharmaceuticals,
        Paints,
        Detergents,
        Cosmetics,
        Agrochemicals,

        // Промежуточные продукты
        Methanol,
        Ethylene,
        Propylene,
        Benzene
    }

    /// <summary>
    /// Типы химических процессов
    /// </summary>
    public enum ChemicalProcess
    {
        Distillation,
        Cracking,
        Polymerization,
        Hydrogenation,
        Oxidation,
        Neutralization,
        Electrolysis,
        Fermentation,
        Crystallization,
        Filtration
    }
}