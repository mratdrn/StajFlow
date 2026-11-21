using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.DTOs;

public class PostingSearchRequest
{
    [FromQuery(Name = "q")]
    [StringLength(200, ErrorMessage = "Arama metni en fazla 200 karakter olabilir.")]
    public string? Keyword { get; set; }

    [FromQuery(Name = "location")]
    [StringLength(250, ErrorMessage = "Konum en fazla 250 karakter olabilir.")]
    public string? Location { get; set; }

    [FromQuery(Name = "workArrangement")]
    [AllowedValues(null, "OnSite", "Hybrid", "Remote", ErrorMessage = "Çalışma biçimi geçersiz.")]
    public string? WorkArrangement { get; set; }
}
