namespace Pantree.Server.Controllers.Search.Providers.FoodDataCentral
{
    /// <summary>
    /// The highest level object describing a search result on FoodData Central
    /// </summary>
    public class FdcSearchResult
    {
        /// <summary>
        /// The foods returned by the search
        /// </summary>
        public FdcFood[]? Foods { get; set; }
    }

    /// <summary>
    /// A FoodData Central representation of a food item
    /// </summary>
    public class FdcFood
    {
        /// <summary>
        /// The FoodData Central ID for the food
        /// </summary>
        public int? FdcId { get; set; }

        /// <summary>
        /// The description of the food, often its name
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The name of the brand selling the food, if any
        /// </summary>
        public string? BrandName { get; set; }

        /// <summary>
        /// The serving size found on the foods nutritional label
        /// </summary>
        public double? ServingSize { get; set; }

        /// <summary>
        /// The unit describing <see cref="ServingSize"/> 
        /// </summary>
        public string? ServingSizeUnit { get; set; }

        /// <summary>
        /// The nutritional information of the food
        /// </summary>
        public FdcNutrient[]? FoodNutrients { get; set; }
    }

    /// <summary>
    /// A nutritional value for a food, typically given in terms of 100g servings of the associated 
    /// <see cref="FdcFood"/> 
    /// </summary>
    public class FdcNutrient
    {
        /// <summary>
        /// The name of the nutritional value being described
        /// </summary>
        public string? NutrientName { get; set; }

        /// <summary>
        /// The unit defining the nutritional measurement
        /// </summary>
        public string? UnitName { get; set; }

        /// <summary>
        /// The amount of the nutrient in terms of <see cref="UnitName"/> 
        /// </summary>
        public double? Value { get; set; }
    }
}
