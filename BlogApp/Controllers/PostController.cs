﻿using BlogApp.Context;
using BlogApp.Entity;
using BlogApp.Migrations;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace BlogApp.Controllers
{
	public class PostController : Controller
	{
		private readonly DataContext _context;

		public PostController(DataContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(string tag)
		{


			var posts = _context.Set<Post>().AsQueryable().Where(i => i.IsActive);

			if (!string.IsNullOrEmpty(tag))
			{
				posts = posts.Where(p => p.Tags.Any(t => t.Url == tag));
			}

			return View(
				new PostViewModel
				{
					Posts = await posts.ToListAsync(),
					//Tags = _context.Tags.ToList()
				}
			);
		}

		public async Task<IActionResult> Details(string url)
		{
			return View(await _context.Post.Include(x => x.Tags).Include(x => x.Comments).ThenInclude(x => x.User).FirstOrDefaultAsync(p => p.Url == url));
		}

		[HttpPost]
		public JsonResult AddComment(int PostId, string Text)
		{

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var username = User.FindFirstValue(ClaimTypes.Name);
			var avatar = User.FindFirstValue(ClaimTypes.UserData);


			var entity = new Comment
			{
				Text = Text,
				PublishedOn = DateTime.Now,
				PostId = PostId,
				UserId = int.Parse(userId ?? "")
			};
			_context.Add(entity);
			_context.SaveChanges();

			//return Redirect("/posts/details/" + Url);

			return Json(new
			{
				username,
				Text,
				entity.PublishedOn,
				avatar

			});
		}

		[Authorize]
		public IActionResult Create()
		{

			return View();
		}
		[Authorize]
		[HttpPost]
		public IActionResult Create(PostCreateViewModel model)
		{

			if (ModelState.IsValid)
			{
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

				_context.Post.Add(new Post
				{
					Title = model.Title,
					Content = model.Content,
					Url = model.Url,
					UserId = int.Parse(userId ?? ""),
					PublishedOn = DateTime.Now,
					Image = "1.jpg",
					IsActive = false
				});
				_context.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(model);
		}

		[Authorize]
		public async Task<IActionResult> List()
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
			var role = User.FindFirstValue(ClaimTypes.Role);


			var posts = _context.Set<Post>().AsQueryable();


			if (string.IsNullOrEmpty(role))
			{
				posts = posts.Where(i => i.UserId == userId);

			}

			return View(await posts.ToListAsync());
		}


		[Authorize]
		public IActionResult Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();


			}
			var post = _context.Post.FirstOrDefault(i => i.PostId == id);
			if (post == null)
			{
				return NotFound();

			}
			return View(new PostCreateViewModel
			{

				PostId = post.PostId,
				Title = post.Title,
				Description = post.Description,
				Content = post.Content,
				Url = post.Url,
				IsActive = post.IsActive
			});
		}
		[Authorize]
		[HttpPost]
		public IActionResult Edit(PostCreateViewModel model)
		{
			if (ModelState.IsValid)
			{
				var entityToUpdate = _context.Post.FirstOrDefault(i => i.PostId == model.PostId);

				entityToUpdate.PostId = model.PostId;
				entityToUpdate.Title = model.Title;
				entityToUpdate.Description = model.Description;
				entityToUpdate.Content = model.Content;
				entityToUpdate.Url = model.Url;
				entityToUpdate.IsActive = model.IsActive;


				if(User.FindFirstValue(ClaimTypes.Role) == "admin")
				{
					entityToUpdate.IsActive = model.IsActive;
				}
				_context.Post.Update(entityToUpdate);
				_context.SaveChanges();
				return RedirectToAction("List");
			}
			return View(model);
		}
	}
}










