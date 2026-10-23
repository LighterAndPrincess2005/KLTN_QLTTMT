using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="Kho"),Route("api/quan-ly/hoa-cu")]
public sealed class QuanLyHoaCuController(ManagementService service):ManagementControllerBase<HoaCu>(service);
