using Acr.UserDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppReservas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FinalizarReservaPage : ContentPage
    {
        HttpClient client;

        public FinalizarReservaPage()
        {
            InitializeComponent();
            logo.Source = ImageSource.FromResource("AppReservas.logo.PNG");
            
            displayDadosReserva();

            displayModoPagamento();
        }

        public void displayDadosReserva()
        {
            var labelCodigoReserva = new Label()
            {
                Text = "Nº " + Variables.dadosReserva.d.code,

                FontSize = 18,

                TextColor = Color.FromHex("#0450cc"),

                FontAttributes = FontAttributes.Bold,

            };
            GridCabecalho.Children.Add(labelCodigoReserva, 1, 1);
            //dados reserva add ao layout
            var labelTituloAtividade = new Label()
            {
                Text = Variables.tituloAtividade,

                FontAttributes = FontAttributes.Bold,

                FontSize = 16,

            };
            LayoutDadosReserva.Children.Add(labelTituloAtividade);

            var dayWeek = Variables.gvWeekDays[(int)Variables.dataReserva.DayOfWeek].Substring(0, 3);

            var labelHorario = new Label()
            {
                Text = dayWeek + ", " + Variables.dataReserva.Day + " de " + Variables.gvMonth[Variables.dataReserva.Month] + " de " + Variables.dataReserva.Year + " das " + Variables.horarioAtividade,

                FontSize = 14,
            };
            LayoutDadosReserva.Children.Add(labelHorario);



            //verificar dados reserva
            for (int i = 0; i < Variables.arrayTarifas.Length; i++)
            {
                var labelTarifa = new Label()
                {
                    Text = Variables.arrayTarifas[i].quantidade + " x " + Variables.arrayTarifas[i].nome + " " + Variables.arrayTarifas[i].valor + Variables.arrayTarifas[i].moeda,
                };
                if (!Variables.arrayTarifas[i].quantidade.Equals("0"))
                {
                    LayoutDadosReserva.Children.Add(labelTarifa);
                }

            }

            var labelTotal = new Label()
            {
                Text = "Total: " + Variables.totalPagar + Variables.arrayTarifas[0].moeda,

                FontSize = 16,

                TextColor = Color.FromHex("#0450cc"),

                HorizontalTextAlignment = TextAlignment.End,

                FontAttributes = FontAttributes.Bold,

            };
            LayoutDadosReserva.Children.Add(labelTotal);


        }





        public void onClickedValidateVoucher(object sender, EventArgs e)
        {
            //Pendente colocar aqui a chamada ao ws para a validação do voucher-------------------------------------------------------

            string entryVoucher = ((Entry)GridVoucher.Children.ElementAt(0)).Text;

            Debug.Write("clicou validar voucher: " + entryVoucher);

            //call ws validate voucher------------------------------------------
            client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(7);

            client.BaseAddress = new Uri("http://fo.qualidade.inmadeira.com");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, "rest/quickbook.asmx/checkVouchers");

            //string param = "Vouchers:'"+ entryVoucher + "::1::0::1::0::0'";

            string param = "{Vouchers:'" + entryVoucher + "::1::0::1::0::0'}";

            request.Content = new StringContent(param,
                       Encoding.UTF8,
               "application/json");


            var response = client.SendAsync(request).Result;

            Debug.Write("\n response" + response);//ver qual é o erro que é retornado e verificar parametrdo

            string data = "";

            if (response.IsSuccessStatusCode)
            {
                //obter o token
                data = response.Content.ReadAsStringAsync().Result;
                Debug.Write("resposta voucher: " + data);

                string[] voucherTemp = JsonConvert.DeserializeObject(data).ToString().Split(new[] { "::" }, StringSplitOptions.None);

                float desconto = float.Parse(voucherTemp[4]);
                string[] tipoVoucherTemp = voucherTemp[5].Split(new[] {"\"\r\n" }, StringSplitOptions.None);
                var tipoVoucher = int.Parse(tipoVoucherTemp[0]);

                Debug.WriteLine("\n desconto: "+ desconto);
                Debug.WriteLine("\n tipo Reserva: " + tipoVoucher);

                Variables.custoComDesconto = Variables.totalPagar;

                //calcular o desconto/
                //existe dois tipos: por percentagem ou por valor absoluto
                if (tipoVoucher == 1)//calculo em percentagem
                {
                    Variables.custoComDesconto = Variables.custoComDesconto * (1 - desconto / 100);
                    Variables.gvIsVoucherValid = true;
                   
                }
                else if (tipoVoucher == 2)//calculo em valor absoluto
                {
                    Variables.custoComDesconto = Variables.custoComDesconto - desconto;
                    Variables.gvIsVoucherValid = true;
                }

                Debug.Write("\n" + Variables.custoComDesconto);

                foreach (var child in LayoutDadosReserva.Children.Reverse())
                {

                    LayoutDadosReserva.Children.Remove(child);
                    
                }

                displayDadosReserva();

                var labelDesconto = new Label()
                {
                    Text = "Total com desconto: " + Variables.custoComDesconto + Variables.arrayTarifas[0].moeda,

                    FontSize = 18,

                    TextColor = Color.FromHex("#0450cc"),

                    HorizontalTextAlignment = TextAlignment.End,

                    FontAttributes = FontAttributes.Bold,

                };
                LayoutDadosReserva.Children.Add(labelDesconto);
            }

        }




        public void displayModoPagamento()
        {
            Debug.Write("asasaasasa: "+ Variables.gvModoPagamento.Length/2);

            Variables.idModoPagamento = Variables.gvModoPagamento[0, 0];

            for (int i = 0; i < Variables.gvModoPagamento.Length/2;i++)
            {
                var btnPagamento = new Button()
                {
                    Text = Variables.gvModoPagamento[i, 1],

                    StyleId = Variables.gvModoPagamento[i, 0],

                    BackgroundColor = Color.FromHex("#FFFFFF"),

                    TextColor = Color.FromHex("#000000"),
                  
                };
                if(btnPagamento.StyleId.Equals(Variables.gvModoPagamento[0, 0]))
                {
                    btnPagamento.BackgroundColor = Color.FromHex("#0946a5");
                }
                LayoutBtnPagamento.Children.Add(btnPagamento);

                btnPagamento.Clicked += (sender, e) =>
                {
                    foreach (var child in LayoutBtnPagamento.Children.Reverse())
                    {
                        var childTypeName = child.GetType().Name;

                        if (childTypeName == "Button")
                        {
                            child.BackgroundColor = Color.FromHex("#FFFFFF");
                        }
                    }

                    btnPagamento.BackgroundColor = Color.FromHex("#0946a5");

                    Variables.idModoPagamento = btnPagamento.StyleId;
                };
            }
            
        }


        //Validação dos campos formulario cliente
        public bool isValideEmail(string email)
        {
            if (email == null || email.Equals(""))
            {
                DisplayAlert("info", "preencher email", "ok");
                return false;
            }
            // Return true if strIn is in valid email format.
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return Regex.IsMatch(email,
                     @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                     @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                     RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));               
            }
            catch
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoEmailNaoVal, "ok");
                return false;
            }
                      
        }

        public bool fieldValidation(string campoForm)
        {
            if (campoForm == null || campoForm.Equals(""))
            {
                DisplayAlert("info","Todos os campos devem ser preenchidos", "ok");
                return false;
            }
            return true;
        }

        public bool isValideData(string formPrimeiroNome, string formApelido, string formTelefone, string formHotel, string formNQuarto, string formNacionalidade, string formContribuite, string formCodPostal, string formMorada, string formObs)
        {
            if (formPrimeiroNome == null || formPrimeiroNome.Equals(""))
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoNomeFactEmFalta, "ok");
                return false;
            }
            else if (formApelido == null || formApelido.Equals(""))
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoApelidoFactEmFalta, "ok");
                return false;
            }
            else if (formTelefone == null || formTelefone.Equals("") || formTelefone.Length !=9)
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoTelefone, "ok");
                return false;
            }
            else if (formHotel == null || formHotel.Equals(""))
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoHotelFalta, "ok");
                return false;
            }
            else if (formNQuarto == null || formNQuarto.Equals(""))
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoQuartoFalta, "ok");
                return false;
            }
            else if (formNacionalidade == null || formNacionalidade.Equals(""))
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoPaisFactEmFalta, "ok");
                return false;
            }
            else if (formContribuite == null || formContribuite.Equals("") || formContribuite.Length != 9)
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoNifNaoVal, "ok");
                return false;
            }
            else if (formCodPostal == null || formCodPostal.Equals(""))
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoCPFactEmFalta, "ok");
                return false;
            }
            else if (formMorada == null || formMorada.Equals(""))
            {
                DisplayAlert("info", Variables.gvMsgTradAvisoLocFactEmFalta, "ok");
                return false;
            }          

            return true;
        }

        public async Task<bool> onClickedConfirmar(object sender, EventArgs e)
        {
            

            /*
             * ----------------------email------------------------
             */
            Entry emailEntry = (Entry) email;

            string inEmail = emailEntry.Text;

            if (!isValideEmail(inEmail)) return false;

            /*
             * --------------dados restantes----------------------
             */
            Entry inPrimeiroNome = (Entry)primeiroNome;
            Entry inApelido = (Entry)apelido;
            Entry inNTelefone = (Entry)nTelefone;
            Entry inHotel = (Entry)hotel;
            Entry inNQuarto = (Entry)nQuarto;
            Entry inNacionalidade = (Entry)nacionalidade;
            Entry inContribuite = (Entry)contribuite;
            Entry inCodPostal = (Entry)codPostal;
            Entry inMorada = (Entry)morada;
            Editor inObs = (Editor)obsInput;

            string formPrimeiroNome = inPrimeiroNome.Text;
            string formApelido = inApelido.Text;
            string formTelefone = inNTelefone.Text;
            string formHotel = inHotel.Text;
            string formNQuarto = inNQuarto.Text;
            string formNacionalidade = inNacionalidade.Text;
            string formContribuite = inContribuite.Text;
            string formCodPostal = inCodPostal.Text;
            string formMorada = inMorada.Text;
            string formObs = inObs.Text;


            if (!isValideData(formPrimeiroNome, formApelido, formTelefone, formHotel, formNQuarto, formNacionalidade, formContribuite, formCodPostal, formMorada, formObs)) return false;



            /*
             * ----------------------concat da string com os dados cliente-----------------------
             */


            string identity = "";
            identity += formPrimeiroNome.Trim();
            identity += '$' + formApelido.Trim();
            identity += '$' + formTelefone.Trim();
            identity += '$' + inEmail.Trim();
            identity += '$' + formHotel;
            identity += '$' + formNQuarto;
            identity += '$' + formNacionalidade;
            identity += '$' + formPrimeiroNome.Trim() + " " + formApelido.Trim() + '$' + formContribuite;
            identity += '$' + formCodPostal;
            identity += '$' + formMorada;
            identity += '$' + Variables.idModoPagamento.Trim();//id do modo de paagameento
            identity += '$' + formObs;

            Debug.WriteLine(identity);


            /*
             * -------------call ws validate voucher--------------------------
             */
            try
            {
                UserDialogs.Instance.ShowLoading("A finalizar a reserva...");
                // your code 
                client = new HttpClient();

                client.Timeout = TimeSpan.FromSeconds(15);

                client.BaseAddress = new Uri("http://fo.qualidade.inmadeira.com");

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string data = "";
                //call com voucher correto -confirmReservationWithVouchers
                if (Variables.gvIsVoucherValid)
                {
                    Entry voucherCode = (Entry)entryVoucher;

                    string voucher = voucherCode.Text;

                    var request = new HttpRequestMessage(HttpMethod.Post, "rest/quickbook.asmx/confirmReservationWithVouchers");

                    string param = "{Code: " + Variables.dadosReserva.d.code + ",Identity: '" + identity + "', Token: '" + Variables.token + "', Vouchers: '0|0||0|" + voucher.Trim() + "'}";



                    request.Content = new StringContent(param,
                           Encoding.UTF8,
                   "application/json");


                    var response = await client.SendAsync(request);

                    Debug.WriteLine("Resposta final: " + response);

                    data = await response.Content.ReadAsStringAsync();

                    var temp = JsonConvert.DeserializeObject(data);

                   
                }
                else
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "rest/quickbook.asmx/confirmReservation");

                    string param = "{Code: " + Variables.dadosReserva.d.code + ",Identity: '" + identity + "', Token: '" + Variables.token + "'}";




                    request.Content = new StringContent(param,
                           Encoding.UTF8,
                   "application/json");


                    var response = await client.SendAsync(request);

                    Debug.WriteLine("Resposta final: "+response.Content);

                    data = await response.Content.ReadAsStringAsync();

                    var temp = JsonConvert.DeserializeObject(data);


                }

               
                UserDialogs.Instance.HideLoading();

                var toastConfig = new ToastConfig("Reserva "+ Variables.dadosReserva.d.code + " efetuada com sucesso");
                toastConfig.SetDuration(3000);
                UserDialogs.Instance.Toast(toastConfig);

                Navigation.InsertPageBefore(new ReservaPage(), this);

                await Navigation.PopAsync();

                

            }
            catch (Exception exc)
            {
                UserDialogs.Instance.HideLoading();

                Debug.WriteLine("exceção finalizar: " + exc.Message);

                var toastConfig = new ToastConfig("Reserva " + Variables.dadosReserva.d.code + " efetuada com sucesso");

                toastConfig.SetDuration(3000);

                UserDialogs.Instance.Toast(toastConfig);

                Navigation.InsertPageBefore(new ReservaPage(), this);

                await Navigation.PopAsync();

                
            }
            return true;

        }



        public void OnClickedCancelar(object sender, EventArgs e)
        {
            DisplayAlert("Info", "A reserva irá ser cancelada", "OK");

            Thread.Sleep(2000);

            Navigation.InsertPageBefore(new ReservaPage(), this);

            Navigation.PopAsync();

        }




        //Preenchinento apenas para debug-------------------------------
        public void onClickedPreencheForm(object sender, EventArgs e)
        {
            Entry inEmail = (Entry)email;
            Entry inPrimeiroNome = (Entry)primeiroNome;
            Entry inApelido = (Entry)apelido;
            Entry inNTelefone = (Entry)nTelefone;
            Entry inHotel = (Entry)hotel;
            Entry inNQuarto = (Entry)nQuarto;
            Entry inNacionalidade = (Entry)nacionalidade;
            Entry inContribuite = (Entry)contribuite;
            Entry inCodPostal = (Entry)codPostal;
            Entry inMorada = (Entry)morada;
            Editor inObs = (Editor)obsInput;

            inPrimeiroNome.Text = "testeNome";
            inApelido.Text = "inApelido";
            inNTelefone.Text = "123456789";
            inHotel.Text = "testeHotel";
            inNQuarto.Text = "111";
            inContribuite.Text = "999999999";
            inCodPostal.Text = "testCodePostal";
            inMorada.Text = "moradaTeste";
            inObs.Text = "teste obstsasasas...";
            inEmail.Text = "teste@teste.com";


        }
        

    }
}