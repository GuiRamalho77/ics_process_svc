namespace InsurerServices.Providers.Liberty.Data
{
    public class ServiceConfigurationIcs
    {
        public Config Config { get; set; }

        #region Regras do Serviço

        public int CD_PESSOA_SEGURADORA { get; set; }
        public int NR_INTEGRACAO { get; set; }
        public int VALOR_MAXIMO { get; set; }
        public int PAGINA { get; set; }
        public string DT_ULTIMA_EXEC_SUCESSO { get; set; }

        #endregion

        public ServiceConfigurationIcs()
        {
            Config = new Config();
        }
    }

    public class Config
    {
        public long NR_INTERVALO { get; set; }
        public int QTD_DADOS { get; set; }
        public string NR_THREADS { get; set; }
        public bool STATUS { get; set; }
        public string DT_PROXIMA_RODADA { get; set; }
        public bool EXECUTA_PERIODO { get; set; }
    }
}