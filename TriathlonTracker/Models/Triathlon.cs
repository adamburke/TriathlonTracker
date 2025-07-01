using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class Triathlon
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RaceName { get; set; } = string.Empty;
        
        [Required]
        public DateTime RaceDate { get; set; }
        
        [Required]
        public string Location { get; set; } = string.Empty;
        
        // Swim Information
        [Required]
        [Range(0, double.MaxValue)]
        public double SwimDistance { get; set; } // in meters or yards
        
        [Required]
        public string SwimUnit { get; set; } = "yards"; // "meters" or "yards"
        
        public TimeSpan SwimTime { get; set; }
        
        public double SwimPace => SwimDistance > 0 ? SwimTime.TotalMinutes / (SwimDistance / 100) : 0; // minutes per 100m or 100yd
        
        // Bike Information
        [Required]
        [Range(0, double.MaxValue)]
        public double BikeDistance { get; set; } // in kilometers or miles
        
        [Required]
        public string BikeUnit { get; set; } = "miles"; // "km" or "miles"
        
        public TimeSpan BikeTime { get; set; }
        
        public double BikePace => BikeDistance > 0 ? BikeDistance / BikeTime.TotalHours : 0; // km/hr or mph
        
        // Run Information
        [Required]
        [Range(0, double.MaxValue)]
        public double RunDistance { get; set; } // in kilometers or miles
        
        [Required]
        public string RunUnit { get; set; } = "miles"; // "km" or "miles"
        
        public TimeSpan RunTime { get; set; }
        
        public double RunPace => RunDistance > 0 ? RunTime.TotalMinutes / (RunUnit == "miles" ? RunDistance : RunDistance * 0.621371) : 0; // always minutes per mile
        
        // Total Information
        public TimeSpan TotalTime => SwimTime + BikeTime + RunTime;
        
        public double TotalDistance
        {
            get
            {
                double total = 0;
                
                // Convert swim distance to km
                if (SwimUnit == "yards")
                {
                    total += (SwimDistance * 0.9144) / 1000; // yards to km
                }
                else
                {
                    total += SwimDistance / 1000; // meters to km
                }
                
                // Convert bike distance to km
                if (BikeUnit == "miles")
                {
                    total += BikeDistance * 1.60934; // miles to km
                }
                else
                {
                    total += BikeDistance;
                }
                
                // Convert run distance to km
                if (RunUnit == "miles")
                {
                    total += RunDistance * 1.60934; // miles to km
                }
                else
                {
                    total += RunDistance;
                }
                
                return total;
            }
        }
        
        // Navigation Properties
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 