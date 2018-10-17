using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net.Http;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading;
using System.ComponentModel;
using Acr.UserDialogs;

namespace AppReservas
{
    public class Variables
    {   //http://fo.qualidade.inmadeira.com/rest/fajabook.htm
        //test with my_device3
        public static string tituloApp = "VMT Madeira";//titulo da toolbar
        public static HttpClient client;
        public static RootObject conjuntoEventos;
        public static RootObject2 conjuntoTarifas;
        public static RootObject3 dadosReserva;
        //public static string inEntidade = "demo";
       // public static string inUser = "user";
        //public static string inPassword = "1234";
        public static string inEntidade = "";
        public static string inUser = "";
        public static string inPassword = "";
        public static string ServiceId = "142";
        public static string ProductCode = "''";
        public static string[] gvWeekDays = { "DOMINGO", "SEGUNDA", "TERÇA", "QUARTA", "QUINTA", "SEXTA", "SÁBADO" };
        public static string[] gvMonth = { "mesZero","Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho","Agosto"
        ,"Setembro","Outubro","Novembro","Dezembro"};
        public static string diaEscolhido = "";
        public static string token = "";
        public static string dayPeriodId = "";
        public static string tituloAtividade = "";
        public static string horarioAtividade = "";
        public static DateTime dataReserva;
        

        public static  string[,] gvModoPagamento = new string[3, 2] { 
            {"1", "Numerário"}, 
            { "16", "Pagamento Diferido" },
            { "3", "Cartão Credito" } };
        public static string idModoPagamento = "";

        public struct Tarifa
        {
            public string nome, quantidade,idTarifa, moeda;
            public float valor;
        }
        public static Tarifa[] arrayTarifas = { };
        public static float totalPagar= 0.0f;
        public static float custoComDesconto = 0.0f;
        public static Dictionary<string, string> dictClients;//idtarifa e quantidade do picekr
        public static string gvMsgTradAvisoPaisFactEmFalta = "Atenção, é necessário indicar o Pais!";
        public static string gvMsgTradAvisoNomeFactEmFalta = "Atenção, é necessário preencher o Nome!";
        public static string gvMsgTradAvisoNifNaoVal = "Atenção, o Nº de Contribuinte não é valido!";
        public static string gvMsgTradAvisoCPFactEmFalta  = "Atenção, o Código Postal é de preenchimento obrigatorio!";
        public static string gvMsgTradAvisoLocFactEmFalta = "Atenção, a Morada é de preenchimento obrigatorio!";
        public static string gvMsgTradAvisoEmailNaoVal = "Atenção, o Email não é válido!";
        public static string gvMsgTradAvisoTelefone = "Atenção, é necessário preencher o campo Telefone de forma correta";
        public static string gvMsgTradAvisoHotelFalta = "Atenção, o hotel é de preenchimento obrigatorio!";
        public static string gvMsgTradAvisoQuartoFalta = "Atenção, o numero do quarto é de preenchimento obrigatorio!";
        public static string gvMsgTradAvisoApelidoFactEmFalta = "Atenção, é necessário preencher o Apelido!";

        public static bool gvIsVoucherValid = false;
        public static ActivityIndicator indicator;
    }


    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        
        public MainPage()
        {

        

            InitializeComponent();

           

            logo.Source = ImageSource.FromResource("AppReservas.logo.PNG");
            RootObject conjuntoEventos = new RootObject();
            RootObject2 conjuntoTarifas = new RootObject2();
            RootObject3 dadosReserva = new RootObject3();
            
           

        }

        public bool getCredentials()
        {
            Entry entidade = Entidade;
            Variables.inEntidade = entidade.Text;

            Entry utilizador = Utilizador;
            Variables.inUser = utilizador.Text;

            Entry password = Password;
            Variables.inPassword = password.Text;

            if(entidade.Text==null|| utilizador.Text == null|| password.Text == null)
            {
                DisplayAlert("Info","É necessário preencher todos os campos","OK");
                return false;
            }
            return true;
        }
       

        public async void onClickedLogin(object sender, EventArgs e)
        {

            //ir para a proxima página caso o login seja efetuado com sucesso
            //verificar sucesso
            //display do erro caso não seja efetuado login!!!
            //colocar o "await" para transição segunda interface
            if (getCredentials())
            {




                try
                {

                    UserDialogs.Instance.ShowLoading("A carregar...");
                    await Authentication();
                    UserDialogs.Instance.HideLoading();

                    /*
                    // Start a task - calling an async function
                    Task<string> callTask = Task.Run(() => Authentication());

                    // Wait for it to finish
                    callTask.Wait();

                    // Get the result
                    string astr = callTask.Result;


                    // Write it our
                    Debug.Write(astr);

                    //if (astr.)


                    UserDialogs.Instance.HideLoading();*/

                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.HideLoading();
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
            else
            {
                return;
            }
            
           
        }

        public async Task<string> Authentication()//step1
        {   //receber os dados do
            //source code:https://forums.asp.net/t/2134228.aspx?How+to+request+Web+API+OAuth+token+using+HttpClient+in+a+C+Windows+application


            //concatenar os dados do login
            string credencias = "{Entity: '" + Variables.inEntidade + "', User: '" + Variables.inUser + "', Password: '" + Variables.inPassword + "'}";
            //"{Entity: 'demo', User: 'user', Password: '1234'}"

            Variables.client = new HttpClient();

            Variables.client.BaseAddress = new Uri("http://fo.qualidade.inmadeira.com");
            Variables.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, "rest/authentication.asmx/login");

            request.Content = new StringContent(credencias,
                                    Encoding.UTF8,
                                    "application/json");

           


            //fazer o request
            var response = await Variables.client.SendAsync(request);
            //Debug.Write(response);

            string data = "";

            //verificar se foi validado
            if (response.IsSuccessStatusCode)
            {                
                //obter o token
                data = await response.Content.ReadAsStringAsync();
          
                var temp = JsonConvert.DeserializeObject(data);

                var tokenTemp = temp.ToString().Split(':');

                string token = tokenTemp[1];
                Variables.token = token.Split('"')[1];

                if(Variables.token.Length == 32)
                {
                    Navigation.InsertPageBefore(new ReservaPage(), this);

                    await Navigation.PopAsync();
                }
                else
                {
                    var toastConfig = new ToastConfig("Dados de login incorretos");
                    toastConfig.SetDuration(3000);
                    UserDialogs.Instance.Toast(toastConfig);
     
                }
                

                return token;
            }
            else
            {
                data = "Impossivel obter token \n";
               
                return data;
            }

            //Debug.WriteLine(data);

            
        }


    }
    
}
