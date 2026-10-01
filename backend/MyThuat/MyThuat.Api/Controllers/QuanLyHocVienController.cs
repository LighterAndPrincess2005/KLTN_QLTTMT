using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="HoSo"),Route("api/quan-ly/hoc-vien")]
public sealed class QuanLyHocVienController(ManagementService service):ManagementControllerBase<HocVien>(service);
