using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="DaoTao"),Route("api/quan-ly/noi-dung-buoi")]
public sealed class QuanLyNoiDungBuoiController(ManagementService service):ManagementControllerBase<NoiDungBuoi>(service);
