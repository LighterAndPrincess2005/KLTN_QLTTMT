using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="DaoTao"),Route("api/quan-ly/loai-khoa-hoc")]
public sealed class QuanLyLoaiKhoaHocController(ManagementService service):ManagementControllerBase<LoaiKhoaHoc>(service);
