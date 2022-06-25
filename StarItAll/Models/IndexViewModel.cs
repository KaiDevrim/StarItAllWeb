namespace StarItAll.Models;

using System.ComponentModel.DataAnnotations;
using Octokit;

public class IndexViewModel
{
    [Required] public string? Username { get; set; }

    public IEnumerable<Repository>? Repositories { get; set; }
    public string? ErrorMessage { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}