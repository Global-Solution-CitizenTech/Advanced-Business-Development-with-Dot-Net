namespace ZelaFix.Application.DTOs;

/// <summary>Link HATEOAS (href / rel / method) para hipermídia RESTful.</summary>
public sealed record Link(string Href, string Rel, string Method);
