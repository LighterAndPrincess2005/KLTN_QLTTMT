using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="HoSo"),Route("api/quan-ly/giao-vien")]
public sealed class QuanLyGiaoVienController(ManagementService service):ManagementControllerBase<GiaoVien>(service);
