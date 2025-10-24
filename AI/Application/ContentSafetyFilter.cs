using System.Text.RegularExpressions;

namespace AI.Application
{
    public class ContentSafetyFilter
    {
        private readonly List<string> _vulgarWords;
        private readonly List<string> _legalRiskKeywords;
        private readonly List<string> _inappropriatePatterns;

        public ContentSafetyFilter()
        {
            // Initialize vulgar words list
            _vulgarWords = new List<string>
            {
                "fuck", "shit", "damn", "bitch", "asshole", "bastard", "crap", "piss",
                "hell", "dick", "cock", "pussy", "whore", "slut", "fag", "nigger",
                "retard", "idiot", "stupid", "moron", "dumb", "hate", "kill", "die"
            };

            // Initialize legal risk keywords
            _legalRiskKeywords = new List<string>
            {
                "illegal", "crime", "steal", "fraud", "scam", "hack", "virus", "malware",
                "terrorist", "bomb", "weapon", "drug", "cocaine", "heroin", "marijuana",
                "suicide", "murder", "rape", "abuse", "harassment", "threat", "violence",
                "copyright", "pirate", "torrent", "warez", "crack", "keygen"
            };

            // Initialize inappropriate patterns
            _inappropriatePatterns = new List<string>
            {
                @"\b\d{4}\s*\d{4}\s*\d{4}\s*\d{4}\b", // Credit card patterns
                @"\b\d{3}-\d{2}-\d{4}\b", // SSN patterns
                @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", // Email patterns
                @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", // IP address patterns
                @"\b(?:password|passwd|pwd)\s*[:=]\s*\w+", // Password patterns
            };
        }

        public SafetyCheckResult CheckContentSafety(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new SafetyCheckResult { IsSafe = true };

            var lowerContent = content.ToLowerInvariant();

            // Check for vulgar words
            foreach (var word in _vulgarWords)
            {
                if (lowerContent.Contains(word))
                {
                    return new SafetyCheckResult 
                    { 
                        IsSafe = false, 
                        Reason = $"Contains inappropriate language: {word}",
                        Severity = SafetySeverity.High
                    };
                }
            }

            // Check for legal risk keywords
            foreach (var keyword in _legalRiskKeywords)
            {
                if (lowerContent.Contains(keyword))
                {
                    return new SafetyCheckResult 
                    { 
                        IsSafe = false, 
                        Reason = $"Contains potentially illegal content: {keyword}",
                        Severity = SafetySeverity.Critical
                    };
                }
            }

            // Check for inappropriate patterns
            foreach (var pattern in _inappropriatePatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    return new SafetyCheckResult 
                    { 
                        IsSafe = false, 
                        Reason = "Contains sensitive information patterns",
                        Severity = SafetySeverity.Critical
                    };
                }
            }

            // Check for excessive caps (potential shouting/aggression)
            var capsRatio = content.Count(c => char.IsUpper(c)) / (double)content.Length;
            if (capsRatio > 0.7 && content.Length > 10)
            {
                return new SafetyCheckResult 
                { 
                    IsSafe = false, 
                    Reason = "Excessive use of capital letters (potential aggression)",
                    Severity = SafetySeverity.Medium
                };
            }

            // Check for repeated characters (potential spam/aggression)
            if (Regex.IsMatch(content, @"(.)\1{4,}"))
            {
                return new SafetyCheckResult 
                { 
                    IsSafe = false, 
                    Reason = "Contains repeated characters (potential spam)",
                    Severity = SafetySeverity.Low
                };
            }

            return new SafetyCheckResult { IsSafe = true };
        }
    }

    public class SafetyCheckResult
    {
        public bool IsSafe { get; set; }
        public string Reason { get; set; } = string.Empty;
        public SafetySeverity Severity { get; set; }
    }

    public enum SafetySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}
