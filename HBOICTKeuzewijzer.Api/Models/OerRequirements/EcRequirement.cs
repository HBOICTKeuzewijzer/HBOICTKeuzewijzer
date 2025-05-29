namespace HBOICTKeuzewijzer.Api.Models.OerRequirements
{
    /// <summary>
    /// Represents a credit (EC) requirement.
    /// </summary>
    public class EcRequirement
    {
        /// <summary>
        /// The number of ECs required.
        /// </summary>
        public int RequiredAmount { get; set; }

        /// <summary>
        /// Indicates whether the ECs must be from the propaedeutic phase (P).
        /// </summary>
        public bool Propaedeutic { get; set; }
    }
}