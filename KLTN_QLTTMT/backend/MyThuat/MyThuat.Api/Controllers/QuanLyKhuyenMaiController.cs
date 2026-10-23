using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="DaoTao"),Route("api/quan-ly/khuyen-mai")]
public sealed class QuanLyKhuyenMaiController(ManagementService service):ManagementControllerBase<KhuyenMai>(service);
