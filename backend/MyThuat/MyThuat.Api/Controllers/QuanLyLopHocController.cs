using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="DaoTao"),Route("api/quan-ly/lop-hoc")]
public sealed class QuanLyLopHocController(ManagementService service):ManagementControllerBase<LopHoc>(service);
