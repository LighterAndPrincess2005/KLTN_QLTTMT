using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyThuat.Api.Models;
using MyThuat.Api.Services;
namespace MyThuat.Api.Controllers;
[ApiController,Authorize(Policy="Kho"),Route("api/quan-ly/dinh-muc")]
public sealed class QuanLyDinhMucHoaCuController(ManagementService service):ManagementControllerBase<DinhMucHoaCu>(service);
