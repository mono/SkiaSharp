using SkiaSharp.Collector.Commands;
using SkiaSharp.Collector.Models;

namespace SkiaSharp.Collector.Services;

/// <summary>
/// Calculates engagement scores for issues based on activity.
/// 
/// Formula: (Comments × 3) + (Reactions × 1) + (Contributors × 2) + (1/DaysSinceActivity × 1) + (1/DaysOpen × 1)
/// 
/// An issue is "hot" if its current score > previous score (from 7 days ago).
/// </summary>
public class EngagementCalculator
{
    private const double CommentWeight = 3.0;
    private const double ReactionWeight = 1.0;
    private const double ContributorWeight = 2.0;
    private const double RecencyWeight = 1.0;
    private const double AgeWeight = 1.0;
    
    private const int HistoricalDays = 7;

    /// <summary>
    /// Calculate engagement score for an item with engagement data.
    /// </summary>
    public EngagementScore CalculateScore(EngagementData engagement, DateTime createdAt)
    {
        var now = DateTime.UtcNow;
        var historicalCutoff = now.AddDays(-HistoricalDays);

        // Current score (all data)
        var currentScore = CalculateScoreAtTime(engagement, createdAt, now, now);

        // Historical score (data from 7+ days ago)
        var previousScore = CalculateScoreAtTime(engagement, createdAt, historicalCutoff, historicalCutoff);

        // Hot if current > previous (trending up)
        var isHot = currentScore > previousScore && currentScore > 5; // Minimum threshold

        return new EngagementScore(
            Math.Round(currentScore, 2),
            Math.Round(previousScore, 2),
            isHot
        );
    }

    private double CalculateScoreAtTime(
        EngagementData engagement, 
        DateTime createdAt, 
        DateTime dataCutoff,
        DateTime referenceTime)
    {
        // Filter data to cutoff time
        var comments = engagement.Comments
            .Where(c => c.CreatedAt <= dataCutoff)
            .ToList();
        
        var reactions = engagement.Reactions
            .Where(r => r.CreatedAt <= dataCutoff)
            .ToList();

        var commentReactions = comments
            .SelectMany(c => c.Reactions)
            .Where(r => r.CreatedAt <= dataCutoff)
            .ToList();

        // Calculate components
        var commentCount = comments.Count;
        var reactionCount = reactions.Count + commentReactions.Count;
        
        // Unique contributors (authors + reactors)
        var contributors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var comment in comments)
            contributors.Add(comment.Author);
        foreach (var reaction in reactions)
            contributors.Add(reaction.User);
        foreach (var reaction in commentReactions)
            contributors.Add(reaction.User);
        var contributorCount = contributors.Count;

        // Time-based factors
        var lastActivity = GetLastActivityTime(engagement, dataCutoff);
        var daysSinceActivity = lastActivity.HasValue 
            ? Math.Max(1, (referenceTime - lastActivity.Value).TotalDays) 
            : 365; // Default to 1 year if no activity
        
        var daysOpen = Math.Max(1, (referenceTime - createdAt).TotalDays);

        // Calculate score
        var score = 
            (commentCount * CommentWeight) +
            (reactionCount * ReactionWeight) +
            (contributorCount * ContributorWeight) +
            (RecencyWeight / daysSinceActivity) +
            (AgeWeight / daysOpen);

        return score;
    }

    private static DateTime? GetLastActivityTime(EngagementData engagement, DateTime cutoff)
    {
        var times = new List<DateTime>();

        foreach (var comment in engagement.Comments.Where(c => c.CreatedAt <= cutoff))
        {
            times.Add(comment.CreatedAt);
            times.AddRange(comment.Reactions.Where(r => r.CreatedAt <= cutoff).Select(r => r.CreatedAt));
        }

        times.AddRange(engagement.Reactions.Where(r => r.CreatedAt <= cutoff).Select(r => r.CreatedAt));

        return times.Count > 0 ? times.Max() : null;
    }
}
