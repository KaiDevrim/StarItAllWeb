namespace StarItAll.Models;

using System.ComponentModel.DataAnnotations;
using Octokit;

public class IndexViewModel
{
    [Required] public string Username { get; set; } = string.Empty;
    public List<Repository> Starred { get; set; } = new();
    public string? ErrorMessage { get; set; } = string.Empty;
}