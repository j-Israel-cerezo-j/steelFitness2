﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaEntidades;
using Validaciones.utils;
using CapaLogicaNegocio.utils;
using CapaLogicaNegocio.Exceptions;
using CapaLogicaNegocio.Adds;
using CapaLogicaNegocio.MessageErrors;
using CapaLogicaNegocio.Lists;
using CapaLogicaNegocio.Deletes;
using CapaLogicaNegocio.RecoverData;
using CapaLogicaNegocio.Updates;
using CapaLogicaNegocio.Tables;
using CapaLogicaNegocio.Selects;
using System.Web;
using System.IO;
using CapaLogicaNegocio.Inserts;
namespace CapaLogicaNegocio.Services
{
    public class BrancheSerevice
    {
        private BrancheAdd brancheAdd = new BrancheAdd();
        private BrancheList brancheList = new BrancheList();
        private BrancheDelete brancheDelete = new BrancheDelete();
        private BrancheRData brancherData = new BrancheRData();
        private BrancheUpdate brancheUpdate = new BrancheUpdate();
        private ImageList imageList = new ImageList();
        private Delete delete = new Delete();
        private Random rd = new Random();
        private DayList dayList = new DayList();    
        private BranchesTable branchesTable=new BranchesTable();
        private SchedulesTable schedulesTable = new SchedulesTable();   
        private ProductTable productTable=new ProductTable();
        public bool add(Dictionary<string, string> request, List<HttpPostedFile> filesList)
        {            
            bool ban = false;
            var camposEmptysOrNull = Validation.isNullOrEmptys(request);
            if (camposEmptysOrNull.Count == 0)
            {
                Branche branche = new Branche();
                branche.nombre = RetrieveAtributes.values(request, "nombre");
                branche.descripcion = RetrieveAtributes.values(request, "description");
                branche.ubicacion = RetrieveAtributes.values(request, "ubicacion");
                int idBranceAdd=brancheAdd.add(branche);
                if (idBranceAdd == 0)
                    throw new ServiceException(MessageErrors.MessageErrors.errorAddingToBranch);

                try
                {
                    foreach (var file in filesList)
                    {
                        string fileName = rd.Next(1, 100000000).ToString() + file.FileName;
                        string path = Images.Save(file, "branches", fileName);
                        string strUnionsFiel = "('" + fileName + "','" + path + "'," + idBranceAdd + ")";
                        Insert.Many(strUnionsFiel, "images");
                    }
                    ban = true;
                }
                catch(ServiceException se)
                {
                    throw new ServiceException(se.getMessage());
                }
                catch (Exception ex)
                {
                    deleteBranche(idBranceAdd.ToString());
                    throw new ServiceException(MessageErrors.MessageErrors.errorAddingImage);
                }
            }
            else
            {
                foreach (var item in camposEmptysOrNull)
                {
                    if (item.Value)
                    {
                        throw new ServiceException(item.Key + " esta vacío");
                    }
                }
            }
            return ban;
        }
        public bool update(Dictionary<string, string> request,string strId, List<HttpPostedFile> filesList)
        {
            bool ban = false;
            var camposEmptysOrNull = Validation.isNullOrEmptys(request);
            if (camposEmptysOrNull.Count == 0)
            {
                bool status =Convert.ToBoolean( RetrieveAtributes.values(request, "statusImajes"));
                Branche branche = new Branche();
                branche.idSucursal = Convert.ToInt32(strId);
                branche.nombre = RetrieveAtributes.values(request, "nombre");
                branche.descripcion = RetrieveAtributes.values(request, "description");
                branche.ubicacion = RetrieveAtributes.values(request, "ubicacion");
                if (status)
                {
                    try
                    {                        
                        delete.deleteWhere("images", "fkSucursal", branche.idSucursal.ToString());
                        foreach (var file in filesList)
                        {
                            string fileName = rd.Next(1, 100000000).ToString() + file.FileName;
                            string path = Images.Save(file, "branches", fileName);
                            string strUnionsFiel = "('" + fileName + "','" + path + "'," + branche.idSucursal + ")";
                            Insert.Many(strUnionsFiel, "images");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ServiceException(MessageErrors.MessageErrors.failedToUpdate);
                    }                    
                }               
                return brancheUpdate.update(branche);
            }
            else
            {
                foreach (var item in camposEmptysOrNull)
                {
                    if (item.Value)
                    {
                        throw new ServiceException(item.Key + " esta vacío");
                    }
                }
            }
            return ban;
        }
        public string jsonImagesByIdBranche(string strId)
        {
            if (strId=="")
            {
                throw new ServiceException(MessageErrors.MessageErrors.idRecordEmpty);
            }

            return Converter.ToJson(imageList.listImagesByIdBranche(Convert.ToInt32(strId)));
        }
        public string jsonBranches()
        {
            return Converter.ToJson(brancheList.listBraches());
        }
        public string jsonTableBranches()
        {
            return Converter.ToJson(branchesTable.table(), "idSucursal").ToString();
        }
        public bool deleteBranche(string strIds)
        {
            try
            {                
                var idsList = Converter.ToList(strIds);
                foreach (var idItem in idsList)
                {
                    var lisImages=imageList.listImagesByIdBranche(Convert.ToInt32(idItem));
                    foreach (var img in lisImages)
                    {
                        Images.Delete("branches",img.nombre);
                    }
                }
                delete.whereIn("images", "fkSucursal", strIds);
                return brancheDelete.delete(strIds);
            }
            catch (Exception e)
            {
                throw new ServiceException(MessageErrors.MessageErrors.errorDeletingBranch);
            }            
        }
        public string jsonRecoverData(string strId)
        {
            if (strId == "")
            {
                throw new ServiceException(MessageErrors.MessageErrors.idRecordEmpty);
            }
            var branches = new List<Branche>();
            branches.Add(brancherData.dataBrancheByIdBranche(Convert.ToInt32(strId)));
            return Converter.ToJson(branches);
        }
        public Branche getBrancheById(string strId)
        {
            if (strId == "")
            {
                throw new ServiceException(MessageErrors.MessageErrors.idRecordEmpty);
            }
            return brancherData.dataBrancheByIdBranche(Convert.ToInt32(strId));
        }
        public List<Image> getImageListById(string strId)
        {
            if (strId == "")
            {
                throw new ServiceException(MessageErrors.MessageErrors.idRecordEmpty);
            }

            return imageList.listImagesByIdBranche(Convert.ToInt32(strId));
        }
        public List<Day> getDays()
        {
            return dayList.listDays();
        }

        public string jsontableSchedulesByIdBrancheTable(string strId)
        {
            if (strId == "")
            {
                throw new ServiceException(MessageErrors.MessageErrors.idRecordEmpty);
            }
            var namesTypeDateTime=new List<string>() { "horaInicio", "horaCierre" };
            return Converter.ToJson(schedulesTable.tableSchedulesByIdBranche(Convert.ToInt32(strId)),true, namesTypeDateTime).ToString();
        }
        public string getProductsByIdBranche(string strId)
        {
            if (strId == "")
            {
                throw new ServiceException(MessageErrors.MessageErrors.idRecordEmpty);
            }
            return Converter.ToJson(productTable.tableByIdBranche(Convert.ToInt32(strId))).ToString();
        }
    }
}