﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;
using System.Data;
namespace CapaLogicaNegocio.Tables
{
    public class SchedulesTable
    {
        private DatosHour datosHour = new DatosHour();
        public DataTable tableSchedules()
        {
            return datosHour.tableHours();
        }
        public DataTable tableSchedulesByIdBranche(int id)
        {
            return datosHour.horariosByIdBranche(id);
        }
    }
}