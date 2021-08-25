using Edocs.Mensageria.Powerdocs;
using Edocs.Mensageria.Powerdocs.Message;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace NF3ePackRabbitMQPublisher
{
    class Program
    {
        private static readonly MensageriaPowerdocs _mensageriaPowerdocs = new MensageriaPowerdocs("127.0.0.1", 5672, "guest", "guest");

        static void Main(string[] args)
        {
            //SendManyDocument(1);
            SendOneDocument(3);
        }

        private static void SendManyDocument(int quantity)
        {
            long seq = 0;

            while (seq < quantity)
            {
                SendOneDocument(seq);
            }
        }

        private static void SendOneDocument(long seq)
        {
            string seqChave = seq.ToString().PadLeft(9, '0');
            string chave = $"3221051504812400380655002{seqChave}1610660235";

            // Autorização do documento
            NF3eDocumentForCreateDto nf3eDocumentForCreateDto = new NF3eDocumentForCreateDto
            {
                Chave = chave,
                CNPJCPFEmitente = "33014556019962",
                Serie = 1,
                Numero = seq,
                NomeEmitente = "Primeiro Emitente Teste",
                CNPJCPFDestinatario = "22222222222222",
                NomeDestinatario = "Primeiro Destinatário Teste",
                DataEmissao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                Status = 4,
                TipoEmissao = 1,
                ValorTotal = 10.00m,
                EmailsPDF = "ruano@inventti.com.br",
                EmailsXML = "ruano@inventti.com.br",
                DocumentoXML = "<nf3eProc />"
            };
            string data = JsonConvert.SerializeObject(nf3eDocumentForCreateDto, Formatting.None);
            PowerdocsMessage powerdocsMessage = PowerdocsMessage.CreateForDocument(seq, EMessageOperationType.Create, data);
            _mensageriaPowerdocs.Publicar(powerdocsMessage, "PowerDocs", "NF3ePack");

            // Ocorrências
            PublicarOcorrenciaParaNF3e(seq, 1, "Integrado no ERP");
            PublicarOcorrenciaParaNF3e(seq, 4, "Autorizado o uso da NF3-e");
            PublicarOcorrenciaParaNF3e(seq, 9, "Envio de email");
            PublicarOcorrenciaParaNF3e(seq, 10, "Impressão");
            PublicarOcorrenciaParaNF3e(seq, 11, "Retorno ERP");

            // Evento fiscal de cancelamento
            PublicarEventoFiscalCancelamento(seq);
            PublicarOcorrenciaParaNF3e(seq, 6, "Cancelamento efetuado com sucesso.");

            //Atualização do documento depois de cancelado
            nf3eDocumentForCreateDto = new NF3eDocumentForCreateDto
            {
                Chave = chave,
                CNPJCPFEmitente = "33014556019962",
                Serie = 1,
                Numero = seq,
                NomeEmitente = "Primeiro Emitente Teste",
                CNPJCPFDestinatario = "22222222222222",
                NomeDestinatario = "Primeiro Destinatário Teste",
                DataEmissao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                Status = 5,
                TipoEmissao = 1,
                ValorTotal = 10.00m,
                EmailsPDF = "ruano@inventti.com.br",
                EmailsXML = "ruano@inventti.com.br",
                DocumentoXML = "<nf3eProc />"
            };
            data = JsonConvert.SerializeObject(nf3eDocumentForCreateDto, Formatting.None);
            powerdocsMessage = PowerdocsMessage.CreateForDocument(seq, EMessageOperationType.Create, data);
            _mensageriaPowerdocs.Publicar(powerdocsMessage, "PowerDocs", "NF3ePack");

            Thread.Sleep(200);
        }

        private static void PublicarOcorrenciaParaNF3e(long seq, int tipo, string mensagem)
        {
            OccurrenceForCreateDto occurrenceForCreateDto = new OccurrenceForCreateDto { DataOcorrencia = DateTime.Now, TipoOcorrencia = tipo, Mensagem = mensagem, Status = 1 };
            string occurrenceData = JsonConvert.SerializeObject(occurrenceForCreateDto, Formatting.None);
            PowerdocsMessage powerdocsMessage = PowerdocsMessage.CreateForDocumentOccurrence(seq, EMessageOperationType.Create, occurrenceData);

            _mensageriaPowerdocs.Publicar(powerdocsMessage, "PowerDocs", "NF3ePack");
        }

        private static void PublicarEventoFiscalCancelamento(long seq)
        {
            FiscalEventForCreateDto fiscalEventForCreateDto = new FiscalEventForCreateDto { NumeroSequencial = seq + 1, DataEvento = DateTime.Now, TipoEvento = 1, Descricao = "Evento Fiscal de Cancelamento", Status = 4 };
            string fiscalEventData = JsonConvert.SerializeObject(fiscalEventForCreateDto, Formatting.None);
            PowerdocsMessage powerdocsMessage = PowerdocsMessage.CreateForFiscalEvent(seq, EMessageOperationType.Create, fiscalEventData);

            _mensageriaPowerdocs.Publicar(powerdocsMessage, "PowerDocs", "NF3ePack");
        }
    }

    public class NF3eDocumentForCreateDto
    {
        public int Serie { get; set; }
        public long Numero { get; set; }
        public string DataEmissao { get; set; }
        public string CNPJCPFEmitente { get; set; }
        public string NomeEmitente { get; set; }
        public string CNPJCPFDestinatario { get; set; }
        public string NomeDestinatario { get; set; }
        public decimal ValorTotal { get; set; }
        public string Chave { get; set; }
        public string DocumentoXML { get; set; }
        public string EmailsPDF { get; set; }
        public string EmailsXML { get; set; }
        public int TipoEmissao { get; set; }
        public int Status { get; set; }
    }

    public class OccurrenceForCreateDto
    {
        public DateTime DataOcorrencia { get; set; }
        public int TipoOcorrencia { get; set; }
        public int Status { get; set; }
        public string Mensagem { get; set; }
        //public string Usuario { get; set; }
        //public int TipoProcesso { get; set; }
        //public string Chave { get; set; }
    }

    public class FiscalEventForCreateDto
    {
        public long NumeroSequencial { get; set; }
        public DateTime DataEvento { get; set; }
        public int TipoEvento { get; set; }
        public string Descricao { get; set; }
        public int Status { get; set; }
        public int TipoProcesso { get; set; }
        public string Chave { get; set; }
    }
}