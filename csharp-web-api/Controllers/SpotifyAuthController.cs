﻿using csharp_web_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace csharp_web_api.Controllers
{
	[Route("api/spotify-auth")]
	[ApiController]
	public class SpotifyAuthController(SpotifyAuthService spotifyAuthService) : ControllerBase
	{
		private readonly SpotifyAuthService _spotifyAuthService = spotifyAuthService;

		[HttpGet("login")]
		public IActionResult Login()
		{
			var scopes = "user-follow-read user-read-private";
			var authUrl = $"https://accounts.spotify.com/authorize?client_id={_spotifyAuthService._clientId}" +
				  $"&response_type=code&redirect_uri={Uri.EscapeDataString(_spotifyAuthService._redirectUri)}" +
				  $"&scope={Uri.EscapeDataString(scopes)}";

			return Redirect(authUrl);
		}

		[HttpGet("callback")]
		public async Task<IActionResult> Callback([FromQuery] string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return BadRequest(new { error = "Authorization code is missing" });
			}

			try
			{
				// Exchange authorization code for access and refresh tokens
				var accessToken = await _spotifyAuthService.GetAccessTokenAsync(code);
				HttpContext.Session.SetString("AccessToken", accessToken);

				return Ok(new { message = "Authorization successful", accessToken });
			} catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("access-token")]
		public async Task<IActionResult> GetAccessToken([FromBody] SpotifyAuthRequest request)
		{
			if (string.IsNullOrEmpty(request.Code))
			{
				return BadRequest(new { error = "Authorization code is missing" });
			}

			try
			{
				var accessToken = await _spotifyAuthService.GetAccessTokenAsync(request.Code);
				HttpContext.Session.SetString("AccessToken", accessToken);
				return Ok(new { accessToken });
			} catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
		{
			if (string.IsNullOrEmpty(request.RefreshToken))
			{
				return BadRequest(new { error = "Refresh token is missing" });
			}

			try
			{
				var newAccessToken = await _spotifyAuthService.RefreshAccessTokenAsync(request.RefreshToken);
				HttpContext.Session.SetString("AccessToken", newAccessToken);
				return Ok(new { accessToken = newAccessToken });
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("logout")]
		public IActionResult Logout()
		{
			HttpContext.Session.Remove("AccessToken");
			HttpContext.Session.Remove("RefreshToken");
			return Ok(new { message = "Logged out successfully" });
		}
	}

	// Request Models
	public class SpotifyAuthRequest
	{
		public required string Code { get; set; }
	}

	public class RefreshTokenRequest
	{
		public required string RefreshToken { get; set; }
	}
}
