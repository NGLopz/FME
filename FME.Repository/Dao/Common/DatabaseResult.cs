using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FME.Repository.Dao.Common
{
    public class DatabaseResult
    {
        public long IdResponseCode { get; set; }

        public string? ResponseCode { get; set; }

        public long EntityId { get; set; }

        public string? Table { get; set; }

        public string? Message { get; set; }

        public enum ResponseCodes
        {
            /// <summary>
            /// Operacion exitosa
            /// </summary>
            Success = 0,

            /// <summary>
            /// Error general de base de datos
            /// </summary>
            GeneralError = 1000,

            /// <summary>
            /// Nombre de la entidad duplicada
            /// </summary>
            DuplicatedName = 1001,

            /// <summary>
            /// Registro no existe
            /// </summary>
            RecordDoesNotExist = 1002,

            /// <sumary>
            /// Abreviatura de entidad duplicada
            /// </sumary>
            DuplicatedAbbreviation = 1004,

            DuplicatedInsertName = 1012,

            DuplicateDescriptionProduct = 1010,

            DuplicateReferentInternalProduct = 1017,

            DuplicateBarCodeProduct = 1016
        }
    }
}
