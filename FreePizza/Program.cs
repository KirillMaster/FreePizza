using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace FreePizza
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Бот для бесплатной пиццы успешно запущен!");



            string[] messages = new string[]
            {
                "Хочу пиццу!",
                "Pizza Pizza",
                "Win",
                "Пиццу плз))",
                "Пожалуйста, я хочу выиграть",
                "piza",
                "pizza",
                "plz",
                "вин",
                "пицца пицца",
                "победа",
                "вин",
                "суши",
                "хочу кушать",
                "кусь",
                "суши суши",
                "изи бризи",
                "easy",
                "good game",
                "gg wp",
                "виннер",
                "я выиграю",
                "скриньте",
                "чекай",
                "олды здесь",
                "шин",
                "winner winner chiken dinner",
            };

            VkApi api = new VkApi();

           
            DateTime time = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            Console.WriteLine($"Текущее время: {time.ToString()}");
            string path = @"C:\diplom\Example.txt";
            File.AppendAllLines(path, new[] { "Init" });

            const long ownerId = -54267882;
            const long postId = 669135; 
            const long myId = 17370187;

            const long timeToCreateComment = 180000;
            const long timeLimit =  241000;
            const int sleepMs = 500;

            api.Authorize(new ApiAuthParams
            {
                AccessToken = "3d9e2dc94053b9bcfd6676f39b968b70cf24b4380be0cd0518cacab46dc189ae3ac8e369e9197f142d784",
            });


            Console.WriteLine("Успешно авторизовались. Начинаем чекать комментарии:");

            var commentsCountParams = new WallGetCommentsParams()
            {
                OwnerId = ownerId,
                PostId = postId,
                Extended = true,
            };


            VkParameters vkParameters = WallGetCommentsParams.ToVkParameters(commentsCountParams);
            vkParameters.Add("sort","desc");


            int currentCommentsCount = 0;
            long? lastCommentUserId = null;
            Stopwatch lastCommentLifeTimeWatch = Stopwatch.StartNew();
            int myCommentsCount  = 0;

            //максимальное время жизни комментария с момента запуска приложения.
            long maxLifeTimeOfComment = 0;


            while (true) {

                try
                { 
                    Stopwatch watchRequestTime = Stopwatch.StartNew();

                    VkResponse raw = api.Call("wall.getComments", vkParameters);
                 
                    Response response = JsonConvert.DeserializeObject<ResponseWrapper>(raw.RawJson).Response;
                    List<Comment> comments = response.Items;
                    Comments result = Comments.FromJson(raw);
                    currentCommentsCount = result.Count;

                    watchRequestTime.Stop();
                    var elapsedMsOnRequest = watchRequestTime.ElapsedMilliseconds;

                

                    if (lastCommentUserId != comments.FirstOrDefault()?.from_id)
                    {
                        lastCommentUserId = comments.FirstOrDefault()?.from_id;

                        Console.ForegroundColor = ConsoleColor.Green;
                        var authorProfile = response.Profiles.Where(x => x.Id == lastCommentUserId.Value).FirstOrDefault();
                        string authorName = authorProfile?.first_name + " " + authorProfile?.last_name;
                        Console.WriteLine($"Новый комментарий! Id автора: {lastCommentUserId}. Автор: { authorName }.Текст: {comments.Where(x => x.from_id == lastCommentUserId).FirstOrDefault()?.Text ?? "Кинул стикер"}");
                        Console.ResetColor();
                        Console.WriteLine();

                        //запускаем новый таймер
                        lastCommentLifeTimeWatch.Stop();
                        lastCommentLifeTimeWatch = Stopwatch.StartNew();
                    }
                    else
                    {
                       long commentLifeTimeMs = lastCommentLifeTimeWatch.ElapsedMilliseconds;

                       if(commentLifeTimeMs > maxLifeTimeOfComment)
                       {
                            maxLifeTimeOfComment = commentLifeTimeMs;
                       }
                       Console.ForegroundColor = ConsoleColor.Yellow;
                       Console.WriteLine($"Время жизни последнего комментария: {commentLifeTimeMs / 1000f} секунды");
                       Console.ResetColor();
                   
                       var random = new Random();

                       if (commentLifeTimeMs > (timeToCreateComment + random.Next(20000)) 
                            && commentLifeTimeMs < timeLimit && lastCommentUserId != myId && lastCommentUserId != null)
                       {

                            int index = random.Next(messages.Length - 1);

                            string message = messages[index];

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Момент истины! Создаю комментарий! Cообщение : {message}");
                        
                            var createCommentRequestTimeWatch = Stopwatch.StartNew();
                            api.Wall.CreateComment(new WallCreateCommentParams()
                            {
                                OwnerId = ownerId,
                                PostId = postId,
                                Message = message,
                            });

                            createCommentRequestTimeWatch.Stop();
                            myCommentsCount++;
                            Console.WriteLine($"Успешно отправил комментарий! Время отправки запроса: " +
                                $"{createCommentRequestTimeWatch.ElapsedMilliseconds / 1000f} секунды");
                            Console.ResetColor();
                        }

                        else if (commentLifeTimeMs > timeLimit && lastCommentUserId == myId)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Winner-winner chicken-Dinner!");
                            DateTime winDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
                            Console.WriteLine($"Время победного комментария: {winDateTime.ToString()}");
                        
                            Console.ReadLine();
                        }

                  
                    }


                    Console.WriteLine($"Время запроса: {elapsedMsOnRequest / 1000f} секунды");
                    Console.WriteLine($"Общее число комментариев: {currentCommentsCount}");
                    Console.WriteLine($"Id юзера, оставившего последний коммент: {lastCommentUserId ?? 0} ");
                    Console.WriteLine($"Всего отправлено моих комментариев: {myCommentsCount}");
                    Console.WriteLine($"Максимальное время жизни комментария на данный момент: {maxLifeTimeOfComment / 1000f} секунды");

               
                    Thread.Sleep(sleepMs);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ПРОИЗОШЛА КАКАЯ-ТО ОШИБКА!!. Переход на следующую итерацию." + ex.Message);
                }
            }
        }
    }
}