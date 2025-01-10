using System;

namespace UnityEngine.Perception.Randomization.Samplers
{
    /// <summary>
    /// Returns normally distributed random values bounded within a specified range
    /// https://en.wikipedia.org/wiki/Truncated_normal_distribution
    /// </summary>
    [Serializable]
    public class NormalSampler : ISampler
    {
        /// <summary>
        /// The mean of the normal distribution to sample from
        /// </summary>
        public float mean;

        /// <summary>
        /// The standard deviation of the normal distribution to sample from
        /// </summary>
        public float standardDeviation;

        /// <summary>
        /// A range bounding the values generated by this sampler
        /// </summary>
        public FloatRange range;

        /// <summary>
        /// Constructs a normal distribution sampler
        /// </summary>
        public NormalSampler()
        {
            range = new FloatRange(-1f, 1f);
            mean = 0;
            standardDeviation = 1;
        }

        ///<inheritdoc/>
#if !SCENARIO_CONFIG_POWER_USER
        [field: HideInInspector]
#endif
        [field: SerializeField]
        public float minAllowed { get; set; }
        ///<inheritdoc/>
#if !SCENARIO_CONFIG_POWER_USER
        [field: HideInInspector]
#endif
        [field: SerializeField]
        public float maxAllowed { get; set; }
        ///<inheritdoc/>
#if !SCENARIO_CONFIG_POWER_USER
        [field: HideInInspector]
#endif
        [field: SerializeField]
        public bool shouldCheckValidRange { get; set; }

        /// <summary>
        /// Constructs a normal distribution sampler
        /// </summary>
        /// <param name="min">The smallest value contained within the range</param>
        /// <param name="max">The largest value contained within the range</param>
        /// <param name="mean">The mean of the normal distribution to sample from</param>
        /// <param name="standardDeviation">The standard deviation of the normal distribution to sample from</param>
        /// <param name="shouldCheckValidRange">Whether the provided min and max values should be used to validate the range provided with <see cref="minAllowed"/> and <see cref="maxAllowed"/></param>
        /// <param name="minAllowed">The smallest min value allowed for this range</param>
        /// <param name="maxAllowed">The largest max value allowed for this range</param>
        public NormalSampler(
            float min, float max, float mean, float standardDeviation, bool shouldCheckValidRange = false, float minAllowed = 0, float maxAllowed = 0)
        {
            range = new FloatRange(min, max);
            this.mean = mean;
            this.standardDeviation = standardDeviation;
            this.shouldCheckValidRange = shouldCheckValidRange;
            this.minAllowed = minAllowed;
            this.maxAllowed = maxAllowed;
        }

        /// <summary>
        /// Generates one sample
        /// </summary>
        /// <returns>The generated sample</returns>
        public float Sample()
        {
            var rng = SamplerState.CreateGenerator();
            return SamplerUtility.TruncatedNormalSample(
                rng.NextFloat(), range.minimum, range.maximum, mean, standardDeviation);
        }

        /// <summary>
        /// Validates that the sampler is configured properly
        /// </summary>
        public void Validate()
        {
            range.Validate();
            CheckAgainstValidRange();
        }

        /// <summary>
        /// Checks if range valid
        /// </summary>
        public void CheckAgainstValidRange()
        {
            if (shouldCheckValidRange && (range.minimum < minAllowed || range.maximum > maxAllowed))
            {
                Debug.LogError($"The provided min and max values for a {GetType().Name} exceed the allowed valid range. Clamping to valid range.");
                range.minimum = Mathf.Clamp(range.minimum, minAllowed, maxAllowed);
                range.maximum = Mathf.Clamp(range.maximum, minAllowed, maxAllowed);
            }
        }
    }
}
