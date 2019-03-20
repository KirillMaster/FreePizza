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
                "Хочу пиццу!", "Pizza Pizza", "Win", "Пиццу плз))",
                "Пожалуйста, я хочу выиграть", "piza", "pizza", "plz",
                "вин", "пицца пицца", "победа", "вин",
                "суши", "хочу кушать", "кусь", "суши суши",
                "изи бризи", "easy", "good game", "gg wp",
                "виннер", "я выиграю", "скриньте", "чекай",
                "олды здесь", "шин", "winner winner chiken dinner", "победителей не судят",
                "я тебе покушать принес", "Суши это дар богов", "мужчины не собаки, на кости не бросаются", "лучший подарок для МУЖЧИНЫ!",
                "Хочу такой завтрак в постель", "кто бы мне такое приготовил", "Ммм как вскусно", "спасибо за фри мил",
                "чую вкус победы", "А можно колу в придачу", "не могу дождаться победы", "чувствую себя Цезарем",
                "А доставка будет?", "я такие раньше заказывал", "подарю их своей маме", "жаль, что 8 марта уже прошло",
                "Лучше конкурса не было!", "жду не дождусь!", "уже пускаю слюнки", "Слюнки текут",
                "представляю оба сета у себя во рту", "а почему не предоставили возможность выбора", "а чек будет?", "столько желающих победить!",
                "Всем удачи!", "желаю приятно подкрепиться победителю!", "я уже чую запах победы", "Нууу когда уже результаты",
                "А кто чекает комменты?", "лол вы правда верите в победу", "лол",
                "лол можете со мной и не тягаться даже", "засекаю комменты на секундомере", "Эх сейчас бы 4 минуты", "блин ну когда уже",
                "це не е реальним", "це не е можливим", "простой люд обманывают", "люди, зачем вы так спамите!",
                "Эх, сейчас бы покушать пицыы", "эх, сейчас бы суши", "Могу поделиться суши с проигравшими ;)", "результаты дня через 2 буду",
                "Наверное, невозможно победить", "а комменты вообще чекают?", "когда вы все уже пойдете спать или работать", "Выживает сильнейший!",
                "Самые голодные будут бороться до конца!",
            };

            VkApi api = new VkApi();

           
            DateTime time = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            Console.WriteLine($"Текущее время: {time.ToString()}");

            const long ownerId = -54267882;
            const long postId = 669135; 
            const long myId = 17370187;

            const long timeToCreateComment = 180000;
            const long timeLimit =  243000;
            const int sleepMs = 500;

            const bool test = false;

            api.Authorize(new ApiAuthParams
            {
                AccessToken = "b19e2a0059b502a00be6318a5f107ba7796bff42fd963963a5fdb74461e102e9b67c4355a40c92bef173a",
            });


            if (test)
            {
                api.Wall.CreateComment(new WallCreateCommentParams()
                {
                    OwnerId = -110029529,
                    PostId = 33888,
                    Message = "TEST",
                });
            }


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
            var authorName = string.Empty;
            string maxLifeTimeOfCommentAuthorName = string.Empty;

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
                        authorName = authorProfile?.first_name + " " + authorProfile?.last_name;
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
                            maxLifeTimeOfCommentAuthorName = authorName;
                       }
                       Console.ForegroundColor = ConsoleColor.Yellow;
                       Console.WriteLine($"Время жизни последнего комментария: {commentLifeTimeMs / 1000f} секунды.");
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
                    Console.WriteLine($"Максимальное время жизни комментария на данный момент: {maxLifeTimeOfComment / 1000f} секунды. " +
                        $"Автор: { maxLifeTimeOfCommentAuthorName }");



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