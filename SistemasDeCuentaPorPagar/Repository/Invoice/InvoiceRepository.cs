﻿

using System.Collections.Generic;
using Dominio.DTO.Invoice;
using Dominio.Interface.Repository.Invoice;
using Dominio.Request;
using Dominio.Request.Invoices;
using Microsoft.EntityFrameworkCore;
using SistemasDeCuentaPorPagar.Controllers;
using SistemasDeCuentaPorPagar.DB_Data_Context;

namespace SistemasDeCuentaPorPagar.Repository.Invoice
{
    public class InvoiceRepository : IInvoiceRep
    {
        private readonly CuentasPorPagarContext _CuentasPorPagarContext;
        public InvoiceRepository(CuentasPorPagarContext cuentasPorPagarContext)
        {
            _CuentasPorPagarContext = cuentasPorPagarContext;
        }

        public async Task<MessageResponse<string>> AddInvoice(InvoiceReq invoice)
        {
            try
            {
                var supplierID = _CuentasPorPagarContext.Suppliers.Where(s => s.RncCedula == invoice.RncOrCedula).Select(s => s.Id).FirstOrDefault();

                if (supplierID <= 0)
                    return new MessageResponse<string>
                    {
                        IsSuccess = false,
                        Message = "El suplidor no existe",
                        StatuCode = 404
                    };


                var lastInvoiceNumber = await _CuentasPorPagarContext.Invoices.Where(i => i.SupplierId == supplierID).Select(i => i.InvoiceNumber).LastOrDefaultAsync();

                await _CuentasPorPagarContext.Invoices.AddAsync(new DB_Data_Context.Invoice
                {
                    SupplierId = supplierID,
                    InvoiceNumber = (Int32.Parse(lastInvoiceNumber) + 1).ToString(),
                    IssueDate = invoice.IssueDate,
                    ExpirationDate = invoice.ExpirationDate,
                    TotalAmount = invoice.TotalAmount,
                    State = invoice.State,
                });

                await _CuentasPorPagarContext.SaveChangesAsync();

                return new MessageResponse<string>
                {
                    IsSuccess = true,
                    Message = "Successful",
                    StatuCode = 200
                };
            }
            catch (Exception ex)
            {
                return new MessageResponse<string>
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido en el servidor",
                    StatuCode = 500
                };
            }
        }

        public async Task<MessageResponse<InvoiceRes>> GetInvoice(string invoiceNumber)
        {
            try
            {
                var res = await (from invoice in _CuentasPorPagarContext.Invoices
                                 join supplier in _CuentasPorPagarContext.Suppliers
                                 on invoice.SupplierId equals supplier.Id
                                 where invoice.InvoiceNumber == invoiceNumber
                                 select new InvoiceRes
                                 {
                                     InvoiceNumber = invoice.InvoiceNumber,
                                     ExpirationDate = invoice.ExpirationDate,
                                     TotalAmount = invoice.TotalAmount,
                                     IssueDate = invoice.IssueDate,
                                     RncOrCedula = supplier.RncCedula,
                                     State = invoice.State
                                 }).FirstOrDefaultAsync();

                return new MessageResponse<InvoiceRes>
                {
                    IsSuccess = true,
                    Payload = res,
                    StatuCode = 200
                };
            }
            catch (Exception ex)
            {
                return new MessageResponse<InvoiceRes>
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido en el servidor",
                    StatuCode = 500
                };
            }
        }

        public async Task<MessageResponse<List<string>>> GetInvoiceNumbers()
        {
            try
            {
                return new MessageResponse<List<string>>
                {
                    IsSuccess = true,
                    Payload = await _CuentasPorPagarContext.Invoices.Select(s => s.InvoiceNumber).ToListAsync(),
                    StatuCode = 200
                };
            }
            catch (Exception ex)
            {
                return new MessageResponse<List<string>>
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido en el servidor",
                    StatuCode = 500
                };
            }
        }

        public async Task<MessageResponse<List<InvoiceRes>>> GetInvoices()
        {
            try
            {
                var res = await (from invoice in _CuentasPorPagarContext.Invoices
                                 join supplier in _CuentasPorPagarContext.Suppliers
                                 on invoice.SupplierId equals supplier.Id
                                 select new InvoiceRes
                                 {
                                     InvoiceNumber = invoice.InvoiceNumber,
                                     ExpirationDate = invoice.ExpirationDate,
                                     TotalAmount = invoice.TotalAmount,
                                     IssueDate = invoice.IssueDate,
                                     RncOrCedula = supplier.RncCedula,
                                     State = invoice.State
                                 }).ToListAsync();

                return new MessageResponse<List<InvoiceRes>>
                {
                    IsSuccess = true,
                    Payload = res,
                    StatuCode = 200
                };
            }
            catch (Exception ex)
            {
                return new MessageResponse<List<InvoiceRes>>
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido en el servidor",
                    StatuCode = 500
                };
            }
        }

        public async Task<MessageResponse<string>> UpdateInvoice(InvoiceReq suppliers, string invoiceNumber)
        {
            try
            {
                var invoice = _CuentasPorPagarContext.Invoices.Where(i => i.InvoiceNumber == invoiceNumber).FirstOrDefault();

                if (invoice == null)
                    return new MessageResponse<string>
                    {
                        IsSuccess = false,
                        Message = "Factura no encontrada",
                        StatuCode = 404
                    };


                suppliers.State = invoice.State;
                suppliers.IssueDate = invoice.IssueDate;
                suppliers.ExpirationDate = invoice.ExpirationDate;
                suppliers.RncOrCedula = invoice.InvoiceNumber;
                suppliers.TotalAmount = invoice.TotalAmount;

                _CuentasPorPagarContext.SaveChanges();

                return new MessageResponse<string>
                {
                    IsSuccess = true,
                    Payload = "Successful",
                    StatuCode = 200
                };

            }
            catch (Exception ex)
            {
                return new MessageResponse<string>
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido un problema en el servidor",
                    StatuCode = 500
                };

            }
        }
    }
}
