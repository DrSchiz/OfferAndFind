using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfferAndFind.Models;
using OfferAndFindAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using System.Web.Helpers;
using System.Xml.Linq;

namespace OfferAndFind.Controllers
{
    public class HomeController : Controller
    {
        public string IP = "192.168.200.12";
        //Авторизация/Регистрация
        public IActionResult AuthReg(string registration, string authorization)
        {
            if (!string.IsNullOrEmpty(registration)) ViewData["Registration"] = registration;
            if (!string.IsNullOrEmpty(authorization)) ViewData["Authorization"] = authorization;
            return View();
        }
        public ViewResult Authorization() => View();
        [HttpPost]
        public async Task<IActionResult> Authorization(User user)
        {
            User authUser = new User();
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Users/Auth", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResp = await response.Content.ReadAsStringAsync();
                            HttpContext.Session.SetString("ApiResp", apiResp);
                            await Authenticate(apiResp);
                            return RedirectToAction("Privacy");
                        }
                        else
                        {
                            return RedirectToAction("AuthReg", new { authorization = "Введены некорректные данные!" });
                        }
                    }
                }
            }
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity Id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(Id));
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("AuthUser");
            return RedirectToAction("AuthReg");
        }

        public ViewResult Registration() => View();
        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            user = new User()
            {
                Firstname = user.Firstname,
                Name = user.Name,
                Patronymic = user.Patronymic,
                Login = user.Login,
                Password = user.Password,
                EMail = user.EMail,
                IdRole = 2,
                IdStatus = 2
            };
            User regUser = new User();

            using (var httpClienthandler = new HttpClientHandler())
            {
                httpClienthandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true;  };
                using (var httpClient = new HttpClient(httpClienthandler))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Users/Reg", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResp = await response.Content.ReadAsStringAsync();
                            HttpContext.Session.SetString("ApiResp", apiResp);
                            await Authenticate(apiResp);
                            regUser = JsonConvert.DeserializeObject<User>(apiResp);
                            return RedirectToAction("Privacy");
                        }
                        else
                        {
                            return RedirectToAction("AuthReg", new { registration = "Введены некорректные данные!" });
                        }
                    }
                }
            }
        }

        //Объявления
        public async Task<IActionResult> Privacy(string selectedValue, int IdType)
        {
            if (string.IsNullOrEmpty(selectedValue)) selectedValue = "offerAndFind";
            if (selectedValue == "create") return RedirectToAction("Create");
            string apiResp = HttpContext.Session.GetString("ApiResp");
            User authUser = new User();
            authUser = JsonConvert.DeserializeObject<User>(apiResp);
            int id = Convert.ToInt32(authUser.IdUser);
            Model1 model = new Model1();
            UserSelectedValue userSV = new UserSelectedValue()
            {
                Value = selectedValue,
                idUser = id,
                type = IdType
            };
            using (var httpClienthandler = new HttpClientHandler())
            {
                httpClienthandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClienthandler))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(userSV), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Ads/GetAds", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            List<CollectionDataModel> collectionData = JsonConvert.DeserializeObject<List<CollectionDataModel>>(apiResponse);
                            model.collectionDataModel = collectionData;
                        }
                    }
                    using (var responseType = await httpClient.GetAsync($"https://{IP}:7045/api/TypeWorks"))
                    {
                        if (responseType.IsSuccessStatusCode)
                        {
                            string apiRespType = await responseType.Content.ReadAsStringAsync();
                            List<TypeWork> type = JsonConvert.DeserializeObject<List<TypeWork>>(apiRespType);
                            model.typeWork = type;
                        }
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ReadAd(int Id)
        {

            CollectionDataModel ad = new CollectionDataModel();
            using (var httpClienthandler = new HttpClientHandler())
            {
                httpClienthandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClienthandler))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(Id), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Ads/GetAd", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            ad = JsonConvert.DeserializeObject<CollectionDataModel>(apiResponse);
                        }
                    }
                }
            }
            return View(ad);
        }

        public async Task<IActionResult> Create()
        {
            List<TypeWork> typeWork = new List<TypeWork>();
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    using (var response = await httpClient.GetAsync($"https://{IP}:7045/api/TypeWorks"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResp = await response.Content.ReadAsStringAsync();
                            typeWork = JsonConvert.DeserializeObject<List<TypeWork>>(apiResp);
                        }
                        else
                        {
                            ViewData["Create"] = "Введены некорректные данные!";
                            return View(ViewData["Create"]);
                        }
                    }
                }
            }
            return View(typeWork);
        }

        public ViewResult AddAd() => View();

        [HttpPost]
        public async Task<IActionResult> AddAd(Ad ad)
        {
            string apiResp = HttpContext.Session.GetString("ApiResp");
            User authUser = new User();
            authUser = JsonConvert.DeserializeObject<User>(apiResp);
            ad = new Ad()
            {
                Header = ad.Header,
                IdUser = authUser.IdUser,
                Text = ad.Text,
                IdStatus = 2,
                IdType = ad.IdType,
                IdType1 = ad.IdType1,
                Salary = ad.Salary 
            };
            Ad createdAd = new Ad();

            using (var httpClienthandler = new HttpClientHandler())
            {
                httpClienthandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClienthandler))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(ad), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Ads", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            createdAd = JsonConvert.DeserializeObject<Ad>(apiResponse);
                            return RedirectToAction("Privacy");
                        }
                        else
                        {
                            TempData["Create"] = "Введены некорректные данные!";
                            return RedirectToAction("Create");
                        }
                        
                    }
                }
            }
        }
        public ViewResult Response() => View();

        [HttpPost]
        public async Task<IActionResult> Response(int id)
        {
            using (var httpClienthandler = new HttpClientHandler())
            {
                httpClienthandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClienthandler))
                {
                    using (var responseGet = await httpClient.GetAsync($"https://{IP}:7045/api/Ads/{id}"))
                    {
                        string apiResponse = await responseGet.Content.ReadAsStringAsync();
                        string apiResp = HttpContext.Session.GetString("ApiResp");
                        User authUser = new User();
                        authUser = JsonConvert.DeserializeObject<User>(apiResp);
                        Ad ad = JsonConvert.DeserializeObject<Ad>(apiResponse);
                        Chat chat = new Chat()
                        {
                            IdUser = ad.IdUser,
                            User2 = authUser.IdUser,
                            IdAd = ad.IdAd
                        };
                        StringContent content = new StringContent(JsonConvert.SerializeObject(chat), Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Chats", content))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return RedirectToAction("Privacy");
                            }
                            else
                            {
                                return RedirectToAction("Privacy");
                            }

                        }
                    }
                }
            }
        }

        //Личный кабинет

        public async Task<IActionResult> Account()
        {
            string apiResp = HttpContext.Session.GetString("ApiResp");
            User user = new User();
            user = JsonConvert.DeserializeObject<User>(apiResp);
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    using (var response = await httpClient.GetAsync($"https://{IP}:7045/api/Users/{user.IdUser}"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string resp = await response.Content.ReadAsStringAsync();
                            User userGet = JsonConvert.DeserializeObject<User>(resp);
                            return View(userGet);
                        }
                        else
                        {
                            return View();
                        }
                    }
                }
            }
        }
        public ViewResult UpdateUser() => View();
        [HttpPost]
        public async Task<IActionResult> UpdateUser(User user)
        {
            string apiResp = HttpContext.Session.GetString("ApiResp");
            User authUser = new User();
            authUser = JsonConvert.DeserializeObject<User>(apiResp);
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    using (var responseGetUser = await httpClient.GetAsync($"https://{IP}:7045/api/Users/{authUser.IdUser}"))
                    {
                        if (responseGetUser.IsSuccessStatusCode)
                        {
                            string resp = await responseGetUser.Content.ReadAsStringAsync();
                            User userGet = JsonConvert.DeserializeObject<User>(resp);
                            userGet.Login = user.Login;
                            userGet.EMail = user.EMail;
                            userGet.Firstname = user.Firstname;
                            userGet.Name = user.Name;
                            userGet.Patronymic = user.Patronymic;
                            userGet.Password = user.Password;
                            StringContent content = new StringContent(JsonConvert.SerializeObject(userGet), Encoding.UTF8, "application/json");
                            using (var response = await httpClient.PutAsync($"https://{IP}:7045/api/Users/{authUser.IdUser}", content))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    return RedirectToAction("Account");
                                }
                            }
                        }
                    }
                    
                }
            }
            return NoContent();
        }

        public async Task<IActionResult> UsersAds()
        {
            List<CollectionDataModel> List = new List<CollectionDataModel>();
            string apiResp = HttpContext.Session.GetString("ApiResp");
            User authUser = new User();
            authUser = JsonConvert.DeserializeObject<User>(apiResp);
            int id = Convert.ToInt32(authUser.IdUser);
            UserSelectedValue userSV = new UserSelectedValue()
            {
                Value = "userAds",
                idUser = id
            };
            using (var httpClienthandler = new HttpClientHandler())
            {
                httpClienthandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClienthandler))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(userSV), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Ads/GetAds", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            List = JsonConvert.DeserializeObject<List<CollectionDataModel>>(apiResponse);
                        }
                    }
                }
            }
            return View(List);
        }

        public async Task<IActionResult> ReadUsersAd(int Id)
        {
            CollectionDataModel ad = new CollectionDataModel();
            List<CollectionDataModel2> chat = new List<CollectionDataModel2>();
            Model2 model2 = new Model2();
            using (var httpClienthandler = new HttpClientHandler())
            {
                httpClienthandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClienthandler))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(Id), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"https://{IP}:7045/api/Ads/GetAd", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            ad = JsonConvert.DeserializeObject<CollectionDataModel>(apiResponse);
                            model2.collectionDataModel = ad;
                        }
                    }
                    int id = Convert.ToInt32(ad.ads.IdAd);
                    StringContent contentChat = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json");
                    using (var responseChat = await httpClient.PostAsync($"https://{IP}:7045/api/Chats/GetUsersReact", contentChat))
                    {
                        if (responseChat.IsSuccessStatusCode)
                        {
                            string apiResponse = await responseChat.Content.ReadAsStringAsync();
                            chat = JsonConvert.DeserializeObject<List<CollectionDataModel2>>(apiResponse);
                            model2.collectionDataModel2 = chat;
                        }
                    }
                }
            }
            return View(model2);
        }

        public ViewResult UpdateAd() => View();
        [HttpPost]
        public async Task<IActionResult> UpdateAd(Ad ad)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    using (var responseGetUser = await httpClient.GetAsync($"https://{IP}:7045/api/Ads/{ad.IdAd}"))
                    {
                        if (responseGetUser.IsSuccessStatusCode)
                        {
                            string resp = await responseGetUser.Content.ReadAsStringAsync();
                            Ad adGet = JsonConvert.DeserializeObject<Ad>(resp);
                            adGet.Header = ad.Header;
                            adGet.Text = ad.Text;
                            adGet.Salary = ad.Salary;
                            StringContent content = new StringContent(JsonConvert.SerializeObject(adGet), Encoding.UTF8, "application/json");
                            using (var response = await httpClient.PutAsync($"https://{IP}:7045/api/Ads/{ad.IdAd}", content))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    return RedirectToAction("UsersAds");
                                }
                            }
                        }
                    }

                }
            }
            return NoContent();
        }
        public ViewResult DeleteAd() => View();
        [HttpPost]
        public async Task<IActionResult> DeleteAd(int Iddelete)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    using (var responseGetUser = await httpClient.DeleteAsync($"https://{IP}:7045/api/Ads/{Iddelete}"))
                    {
                        if (responseGetUser.IsSuccessStatusCode)
                        {
                            return RedirectToAction("UsersAds");
                        }
                    }
                }
            }
            return NoContent();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}