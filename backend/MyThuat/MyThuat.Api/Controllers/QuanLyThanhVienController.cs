using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="HoSo"),Route("api/quan-ly/thanh-vien")]
public sealed class QuanLyThanhVienController(ManagementService service):ManagementControllerBase<ThanhVien>(service);
