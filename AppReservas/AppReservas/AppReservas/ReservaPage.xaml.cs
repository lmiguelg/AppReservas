using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using Acr.UserDialogs;

namespace AppReservas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReservaPage : ContentPage
    {
        HttpClient client;

        public ReservaPage()
        {
            InitializeComponent();
            ToolbarItem toolbar = new ToolbarItem();
            toolbar.Text = Variables.tituloApp;

            try
            {
                // Start a task - calling an async function
                Task<string> callTask = Task.Run(() => GetTimeTableService());

                // Wait for it to finish
                callTask.Wait();

                // Get the result
                string astr = callTask.Result;

                // Write it our
                //Debug.Write(astr);

                DisplayDias();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }


        }

        public async Task<string> GetTimeTableService()//step1
        {
            string parameters = "{ServiceId:" + Variables.ServiceId + ", ProductCode:" + Variables.ProductCode + "}";

            client = new HttpClient();

            client.BaseAddress = new Uri("http://fo.qualidade.inmadeira.com");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, "rest/timetable.asmx/ListWeek");

            request.Content = new StringContent(parameters, Encoding.UTF8, "application/json");

            //fazer o request
            var response = await client.SendAsync(request);
            //Debug.Write(response);

            string data = "";



            //verificar se foi validado
            if (response.IsSuccessStatusCode)
            {
                //obter o token
                data = await response.Content.ReadAsStringAsync();

                var temp = JsonConvert.DeserializeObject(data);


                Variables.conjuntoEventos = JsonConvert.DeserializeObject<RootObject>(data);
                var tstdsds = Variables.conjuntoEventos;
                //Debug.Write("aqui");
                //Debug.Write(Variables.conjuntoEventos.d.startDate);
                //var tokenTemp = temp.ToString().Split(':');

                // string token = tokenTemp[1];

                //return token;
                return temp.ToString();
            }
            else
            {
                data = "Não foi possível obter o token";

                return data;
            }



        }


        public void DisplayDias()
        {
            DateTime dataHoje = DateTime.Today;

            var btnHoje = new Button
            {
                Text = "Hoje \n" + dataHoje.Day + Variables.gvMonth[dataHoje.Month].Substring(0, 3),

                Margin = new Thickness(-1, 1),

                HeightRequest = 35,

                TextColor = Color.FromHex("0450cc"),

                FontAttributes = FontAttributes.Bold,

                CornerRadius = 5,

            };
            if (Device.RuntimePlatform == Device.Android)//apenas ajusta o tamanho para android
            {
                btnHoje.FontSize = 8;

            }
            btnHoje.BackgroundColor = Color.FromHex("#aaccff");

            btnHoje.BindingContext = dataHoje;

            btnHoje.Clicked += (sender, eventArgs) =>
            {
                foreach (var child in LayoutRoot.Children.Reverse())
                {
                    var childTypeName = child.GetType().Name;
                    if (childTypeName == "Button")
                    {
                        child.BackgroundColor = Color.FromHex("#aaccff");
                    }
                }
                btnHoje.BackgroundColor = Color.FromHex("#0946a5");
                getAtividades(dataHoje);
            };
            //Debug.Write(btnHoje.BindingContext);
            LayoutRoot.Children.Add(btnHoje, 0, 0);

            Grid.SetRowSpan(btnHoje, 2);


            for (int col = 1; col < 15; col++)
            {

                DateTime nextDate = dataHoje.AddDays(col);
                //Debug.Write(nextDate);

                var btn = new Button()
                {
                    Text = Variables.gvWeekDays[(int)nextDate.DayOfWeek].Substring(0, 3) + "\n" + nextDate.Day + Variables.gvMonth[nextDate.Month].Substring(0, 3),



                    Margin = new Thickness(-1, 1),

                    FontSize = 12,

                    TextColor = Color.FromHex("0450cc"),

                    FontAttributes = FontAttributes.Bold,

                    HeightRequest = 60,

                    CornerRadius = 5,


                };
                if (Device.RuntimePlatform == Device.Android)//apenas ajusta o tamanho para android
                {
                    btn.FontSize = 6;

                    btn.HeightRequest = 45;
                }

                btn.BackgroundColor = Color.FromHex("#aaccff");

                btn.BindingContext = nextDate;


                btn.Clicked += (sender, eventArgs) =>
                {
                    foreach (var child in LayoutRoot.Children.Reverse())
                    {
                        var childTypeName = child.GetType().Name;
                        if (childTypeName == "Button")
                        {
                            child.BackgroundColor = Color.FromHex("#aaccff");
                        }
                    }
                    btn.BackgroundColor = Color.FromHex("#0946a5");
                    getAtividades(nextDate);
                };

                if (col < 4)
                {
                    LayoutRoot.Children.Add(btn, col, 0);
                }
                else if (col < 7)
                {
                    LayoutRoot.Children.Add(btn, col - 3, 1);
                }
                else if (col < 11)
                {
                    LayoutRoot.Children.Add(btn, col - 7, 2);
                }
                else
                {
                    LayoutRoot.Children.Add(btn, col - 11, 3);
                }
            }


        }
        //mostra os produtos para aquele dia-passa como parametro a data escolhida
        public void getAtividades(DateTime thisDay)
        {
            Variables.dataReserva = thisDay;
            int nDias = 0;
            try
            {
                nDias = Variables.conjuntoEventos.d.dayList.Count;
            }
            catch (Exception e)
            {
                Debug.Write("Erro getAtividades" + e.Message);
            }


            LayoutEscolhaProduto.HeightRequest = 40;//Adaptar para o android e ios

            //remover os pickers já existentes
            foreach (var child in LayoutTarifas.Children.Reverse())
            {
                var childTypeName = child.GetType().Name;

                if (childTypeName == "Picker")
                {
                    LayoutTarifas.Children.Remove(child);
                }
                var childTypeName2 = child.GetType().Name;

                if (childTypeName2 == "Label")
                {
                    LayoutTarifas.Children.Remove(child);
                }
                var childTypeName3 = child.GetType().Name;

                if (childTypeName2 == "Button")
                {
                    LayoutTarifas.Children.Remove(child);
                }
            }

            for (int i = 0; i < nDias; i++)
            {
                if (Variables.conjuntoEventos.d.dayList[i].day.Date == thisDay)
                {
                    //remover caso exista, botões desta secção antes de inserir os novos botões
                    cleanArea(GridProdutos, "Button");

                    int row = 0;
                    int col = 0;
                    for (int j = 0; j < Variables.conjuntoEventos.d.dayList[i - 1].events.Count; j++)
                    {
                        Debug.Write("Eventos:" + j + " " + Variables.conjuntoEventos.d.dayList[i - 1].events[j].productName + "\n");
                        //colocar os eventos em butoes no xaml
                        var btnProdutos = new Button()
                        {

                            Text = Variables.conjuntoEventos.d.dayList[i - 1].events[j].productName + '\n'
                            + Variables.conjuntoEventos.d.dayList[i - 1].events[j].startTime + " às "
                            + Variables.conjuntoEventos.d.dayList[i - 1].events[j].endTime,

                            ClassId = Variables.conjuntoEventos.d.dayList[i - 1].events[j].productName + '|' + Variables.conjuntoEventos.d.dayList[i - 1].events[j].startTime + " às "
                            + Variables.conjuntoEventos.d.dayList[i - 1].events[j].endTime,

                            FontSize = 14,

                            TextColor = Color.FromHex("0450cc"),

                            FontAttributes = FontAttributes.Bold,

                            HeightRequest = 80,

                            BackgroundColor = Color.FromHex("aaccff"),

                            CornerRadius = 5,
                        };
                        btnProdutos.StyleId = Variables.conjuntoEventos.d.dayList[i - 1].events[j].productId.ToString() + '/' + Variables.conjuntoEventos.d.dayList[i - 1].events[j].consumptionPeriodId.ToString();



                        //btnProdutos.BindingContext = Variables.conjuntoEventos.d.dayList[i - 1].events[j].consumptionPeriodId;
                        if (Device.RuntimePlatform == Device.Android)//apenas ajusta o tamanho para android
                        {
                            btnProdutos.HeightRequest = 95;
                            btnProdutos.FontSize = 12;
                        }
                        //adicionar ao grid
                        GridProdutos.Children.Add(btnProdutos, col, row);
                        col++;
                        if (col == 3)
                        {
                            col = 0;
                            row++;
                        }
                        btnProdutos.Clicked += (sender, eventArgs) =>
                        {
                            foreach (var child in GridProdutos.Children.Reverse())
                            {
                                var childTypeName = child.GetType().Name;
                                if (childTypeName == "Button")
                                {
                                    child.BackgroundColor = Color.FromHex("#aaccff");
                                }
                            }
                            btnProdutos.BackgroundColor = Color.FromHex("#0946a5");
                            string x = btnProdutos.StyleId;
                            Variables.tituloAtividade = btnProdutos.ClassId.Split('|')[0];
                            Variables.horarioAtividade = btnProdutos.ClassId.Split('|')[1];
                            getTarifas(x, thisDay);
                        };
                    }
                }
            }
        }


        public void getTarifas(string x, DateTime dataSelecionada)//,int dayPeriodId, DateTime dataSelecionada)
        {
            
            
            LayoutTarifas1.HeightRequest = 60;
            LayoutTarifas.HeightRequest = 400;





            //remover os pickers já existentes
            foreach (var child in LayoutTarifas.Children.Reverse())
            {
                var childTypeName = child.GetType().Name;

                if (childTypeName == "Picker")
                {
                    LayoutTarifas.Children.Remove(child);
                }
                var childTypeName2 = child.GetType().Name;

                if (childTypeName2 == "Label")
                {
                    LayoutTarifas.Children.Remove(child);
                }
                var childTypeName3 = child.GetType().Name;

                if (childTypeName2 == "Button")
                {
                    LayoutTarifas.Children.Remove(child);
                }
            }



            //remover label

            string[] x1 = x.Split('/');

            string productId = x1[0];

            string periodId = x1[1];

            string[] dataTemp = dataSelecionada.ToString().Split(' ')[0].Split('/');

            if (dataTemp[0].Length == 1)
            {
                dataTemp[0] = '0' + dataTemp[0];
            }
            if (dataTemp[1].Length == 1)
            {
                dataTemp[1] = '0' + dataTemp[1];
            }
            string dataFinal = dataTemp[2] + '-' + dataTemp[0] + '-' + dataTemp[1];
            try
            {
                UserDialogs.Instance.ShowLoading("A obter tarifas...");
                //são estes quatro necessarios para fazer a chamada http client para obter as tarifas
                Debug.Write("productId: " + productId);
                Debug.Write("\n periodId: " + periodId);
                Debug.Write("\n dataSelecionada: " + dataFinal);
                Debug.Write("\n token: " + Variables.token);
                string parametros = "ProductId:" + productId + ",PeriodId:" + periodId + ",BookDate:'" + dataFinal + "', Token:'" + Variables.token + "'";
                //string parametros = "ProductId:548,PeriodId:17380,BookDate:'2018-08-10', Token:'" + Variables.token + "'";


                //obter tarifas
                client = new HttpClient();

                client.Timeout = TimeSpan.FromSeconds(15);

                client.BaseAddress = new Uri("http://fo.qualidade.inmadeira.com");

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage(HttpMethod.Post, "rest/pricetable.asmx/ListByDate");

                request.Content = new StringContent("{" + parametros + "}",
                                Encoding.UTF8,
                         "application/json");


                var response = client.SendAsync(request).Result;

                string data = "";

                if (response.IsSuccessStatusCode)
                {
                    data = response.Content.ReadAsStringAsync().Result;

                    var data3333 = JsonConvert.DeserializeObject(data).ToString();
                    Debug.Write(data3333);

                    Variables.conjuntoTarifas = JsonConvert.DeserializeObject<RootObject2>(data);
                    Variables.arrayTarifas = new Variables.Tarifa[Variables.conjuntoTarifas.d.tariffs.Count];
                    //limpar tarifas caso existão
                    Variables.dictClients = new Dictionary<string, string>();

                    for (int i = 0; i < Variables.conjuntoTarifas.d.tariffs.Count; i++)
                    {
                        Variables.dictClients.Add(Variables.conjuntoTarifas.d.tariffs[i].tariffId.ToString(), "0");
                        //Debug.Write("key value" + Variables.DictClients);

                        Variables.arrayTarifas[i].idTarifa = Variables.conjuntoTarifas.d.tariffs[i].tariffId.ToString();
                        Variables.arrayTarifas[i].nome = Variables.conjuntoTarifas.d.tariffs[i].name;
                        Variables.arrayTarifas[i].valor = Variables.conjuntoTarifas.d.tariffs[i].value;
                        Variables.arrayTarifas[i].moeda = Variables.conjuntoTarifas.d.tariffs[i].coin;

                        Debug.WriteLine(Variables.arrayTarifas[i].valor = Variables.conjuntoTarifas.d.tariffs[i].value);

                        var labelTarifas = new Label()
                        {
                            Text = Variables.conjuntoTarifas.d.tariffs[i].name + " " + Variables.conjuntoTarifas.d.tariffs[i].value + Variables.conjuntoTarifas.d.tariffs[i].coin,
                            FontSize = 14,
                            TextColor = Color.FromHex("#0450cc"),
                        };
                        LayoutTarifas.Children.Add(labelTarifas);


                        var pickerTarifas = new Picker()
                        {
                            //Title = Variables.conjuntoTarifas.d.tariffs[i].name,                          

                            TextColor = Color.FromHex("#0450cc"),

                            SelectedItem = "0",

                            StyleId = Variables.conjuntoTarifas.d.tariffs[i].dayPeriodId.ToString(),

                            ClassId = Variables.conjuntoTarifas.d.tariffs[i].tariffId.ToString(),

                        };


                        pickerTarifas.Items.Add("0");
                        pickerTarifas.Items.Add("1");
                        pickerTarifas.Items.Add("2");
                        pickerTarifas.Items.Add("3");
                        pickerTarifas.Items.Add("4");
                        pickerTarifas.Items.Add("5");
                        pickerTarifas.Items.Add("6");
                        pickerTarifas.Items.Add("7");
                        pickerTarifas.Items.Add("8");
                        pickerTarifas.Items.Add("9");
                        pickerTarifas.Items.Add("10");

                        LayoutTarifas.Children.Add(pickerTarifas);

                        pickerTarifas.SelectedIndexChanged += (sender, e) =>
                        {
                            Variables.dayPeriodId = pickerTarifas.StyleId;
                            //OnPickerChanged(sender,e);
                            Picker varPicker = (Picker)sender;

                            var valuePicker = varPicker.SelectedIndex.ToString();

                            string valPickerStr = varPicker.ClassId.ToString();

                            //Variables.clients[i] = valPickerStr;
                            if (Variables.dictClients.ContainsKey(valPickerStr) == true)
                            {
                                Variables.dictClients[valPickerStr] = valuePicker;
                            }

                        };


                    }

                    var btnReservar = new Button()
                    {
                        Text = "Reservar",

                        TextColor = Color.FromHex("#FFFFFF"),

                        FontAttributes = FontAttributes.Bold,

                        BackgroundColor = Color.FromHex("#1f5fc6"),

                        WidthRequest = 100,

                        HeightRequest = 60,

                        HorizontalOptions = LayoutOptions.Center,

                    };
                    LayoutTarifas.Children.Add(btnReservar);
                    btnReservar.Clicked += async(sender, eventArgs) =>
                    {
                       await onClickedReserva(dataFinal, Variables.dayPeriodId);
                    };
                   
                    //var tarifa1 = tarifasEscolhidas.d2.tariffs;

                    //Debug.Write("******************* " + Variables.);
                }
                else
                {
                    
                    data = "Não foi possível";

                    //return data;
                }
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception e)
            {
                UserDialogs.Instance.HideLoading();

                Debug.Write("exceção reservaPage " + e.Message);
            }

        }


        public async Task onClickedReserva(string bookDate, string dayPeriodId)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("A carregar...");

                string idQuantidadTarifa = "";//string necessario para chamad ao WS

                foreach (KeyValuePair<string, string> pair in Variables.dictClients)
                {
                    // Debug.Write("\n "+pair.Key.ToString() + "  -  " + pair.Value.ToString());
                    if (idQuantidadTarifa.Equals(""))
                    {
                        idQuantidadTarifa = pair.Key + '$' + pair.Value;
                    }
                    else
                    {
                        idQuantidadTarifa = idQuantidadTarifa + '|' + pair.Key + '$' + pair.Value;
                    }
                    for (int j = 0; j < Variables.arrayTarifas.Length; j++)
                    {
                        if (pair.Key.Equals(Variables.arrayTarifas[j].idTarifa))
                        {
                            Variables.arrayTarifas[j].quantidade = pair.Value;
                        }
                    }


                }
                Debug.Write(idQuantidadTarifa);

                string param = "{BookDate:'" + bookDate + "',Period:" + dayPeriodId + ",Clients:'" + idQuantidadTarifa + "', Token:'" + Variables.token + "'}";

                Debug.Write("\n params" + param);

                client = new HttpClient();

                client.Timeout = TimeSpan.FromSeconds(20);

                client.BaseAddress = new Uri("http://fo.qualidade.inmadeira.com");

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage(HttpMethod.Post, "rest/quickbook.asmx/simple");

                request.Content = new StringContent(param,
                           Encoding.UTF8,
                   "application/json");


                var response = await client.SendAsync(request);

                Debug.Write("\n response" + response);//ver qual é o erro que é retornado e verifiar parametrdo

                string data = "";

                if (response.IsSuccessStatusCode)
                {
                    //obter o token
                    data = await response.Content.ReadAsStringAsync();



                    var temp = JsonConvert.DeserializeObject(data);

                    Variables.dadosReserva = JsonConvert.DeserializeObject<RootObject3>(data);

                    var teste = Variables.dadosReserva;

                    Debug.WriteLine("resposta rsesrva: " + teste.d);

                    UserDialogs.Instance.HideLoading();

                    if (teste.d.code.Equals("") || teste.d.errorCode == -5)
                    {
                        await DisplayAlert("Info", "Impossível fazer reserva.\n Atividade indisponivel", "OK");

                        Navigation.InsertPageBefore(new ReservaPage(), this);

                        await Navigation.PopAsync();
                    }
                    else
                    {
                        Variables.totalPagar = (float)teste.d.fullValue;

                        Navigation.InsertPageBefore(new FinalizarReservaPage(), this);

                        await Navigation.PopAsync();

                    }

                }
                else
                {
                    data = "Impossivel fazer reserva \n";
                    Debug.Write(data);
                    UserDialogs.Instance.HideLoading();
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.ToString());

                Debug.Write("exceção reserva-step 6: " + e.Message);

                UserDialogs.Instance.HideLoading();

                await DisplayAlert("Info", "Impossível fazer reserva. \n Tente Reservar outro dia.", "OK");

            }



        }



        //Limpa uma determinada área dependendo dos childrens que queremos eliminar
        //parametros: area= x:Name da area a limpar(stackLayout ou grid)
        //parametros: childType= tipo dos objetos a eliminar(ex: button)
        public void cleanArea(Grid area, string childType)
        {
            foreach (var child in area.Children.Reverse())
            {
                var childTypeName = child.GetType().Name;
                if (childTypeName == childType)
                {
                    GridProdutos.Children.Remove(child);
                }
            }
        }


    }
}