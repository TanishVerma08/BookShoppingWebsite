using Dapper;
using EcommProject.DataAccess.Repository.IRepository;
using EcommProject.Models;
using EcommProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            // return Json(new { data = _unitOfWork.CoverType.GetAll() });
            return Json(new { data = _unitOfWork.SP_CALL.List<CoverType>(SD.SP_GetCoverTypes) });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            DynamicParameters dynamic = new DynamicParameters();
            dynamic.Add("id", id);
            //var coverTypeInDb = _unitOfWork.CoverType.Get(id);
            var coverTypeInDb = _unitOfWork.SP_CALL.OneRecord<CoverType>(SD.SP_GetCoverType, dynamic);
            if (coverTypeInDb == null)
                return Json(new { success = false, message = "Something Went Wrong !!!" });
            _unitOfWork.SP_CALL.Execute(SD.SP_DeleteCoverTypes, dynamic);
            // _unitOfWork.CoverType.Remove(coverTypeInDb);
            // _unitOfWork.Save();
            return Json(new { success = true, message = "Data Deleted Successfully !!!" });
        }
        #endregion

        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType); //Create
            //Edit
            DynamicParameters p = new DynamicParameters();
            p.Add("id", id.GetValueOrDefault());
            coverType = _unitOfWork.SP_CALL.OneRecord<CoverType>(SD.SP_GetCoverType,p);
         // coverType = _unitOfWork.CoverType.Get(id.GetValueOrDefault());
            if (coverType == null) return NotFound();
            return View(coverType);
        }
        [HttpPost]
        public IActionResult Upsert(CoverType coverType)
        {
            DynamicParameters dynamic = new DynamicParameters();
            dynamic.Add("name", coverType.Name);
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                // _unitOfWork.CoverType.Add(coverType);
                _unitOfWork.SP_CALL.Execute(SD.SP_CreateCoverType, dynamic);
            else
            {  // _unitOfWork.CoverType.Update(coverType);
                dynamic.Add("id", coverType.Id);
                _unitOfWork.SP_CALL.Execute(SD.SP_UpdateCoverTypes, dynamic);
            }
            // _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
