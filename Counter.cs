using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Counter
{
    private static readonly string urlBeforeGoON = "https://vk.com/friends?act=find";
    static string? _profileName = "";
    static string? _logFileName = "";
    static RemoteWebDriver? _driver;

    public static async Task<string> CountFriendsAsync(RemoteWebDriver driver, string profileName, string logFileName)
    {
        _profileName = profileName;
        _logFileName = logFileName;
        _driver = driver;

        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;

        await Task.Run(() =>
        {
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        });

        await Processes.CheckRunningChromeAsync();
        //Processes.CheckRunningChrome();

        await CheckPageAsync();

        try
        {
            // Проверяем есть ли этот элемент на странице, если да, то мы на главной
            var vkLightBox = driver.FindElement(By.CssSelector(".vkuiPopoutRoot__modal"));
        }
        catch (Exception)
        {
            // Если нет, то переключаемся на главную
            var myPageLink = driver.FindElement(By.XPath("//a[contains(span/text(),'Моя страница')]"));
            var actions = new Actions(driver);
            actions.MoveToElement(myPageLink).Click().Perform();
            await Task.Delay(2000);            
        }

        // Ожидаем загрузки страницы
        await Task.Run(() =>
        {
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        });

        string? countText = "";
        try
        {
            // Получаем элемент в котором указано количество друзей
            var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@class='vkuiHeader__content-in' and text()='Друзья']/following-sibling::span[@class='vkuiHeader__indicator vkuiCaption vkuiCaption--l-1 vkuiCaption--w-1']")));

            countText = element.Text;
        }
        catch (Exception)
        {
            string message = $"В профиле {_profileName} на {DateTime.Now} не удалось получить количество друзей";
            LogManager.LogMessage(message, _logFileName);

            _driver.Dispose();
            await Task.Delay(1000);
        }

        string message5 = $"В профиле {_profileName} на {DateTime.Now} || {countText} || друзей";
        LogManager.LogMessage(message5, _logFileName);

        await Task.Delay(500);
        driver.Navigate().GoToUrl(urlBeforeGoON);
        await Task.Delay(1000);

        return countText;
    }

    public static async Task CheckPageAsync()
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            await Task.Run(() =>
            {
                wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            });
        }
        catch (Exception)
        {
            string message = $"Не удалось дождаться загрузки страницы: {_profileName}";
            LogManager.LogMessage(message, _logFileName);

            _driver?.Dispose();
            await Task.Delay(1000);
        }

        // Проверяем Url на предмет блокировки
        if (_driver.Url.Contains("blocked"))
        {
            string message = $"Этот аккаунт заблокирован: {_profileName}";
            LogManager.LogMessage(message, _logFileName);

            _driver.Dispose();
            await Task.Delay(1000);
        }

        // Уходим если пустая адресная строка
        if (String.IsNullOrEmpty(_driver.Url))
        {
            _driver.Dispose();
            await Task.Delay(1000);
        }

        // Проверяем страницу на предмет popup с предложением о красивом имени
        try
        {
            IWebElement element = _driver.FindElement(By.XPath("//div[@class='box_layout' and @onclick='boxQueue.skip=true;']"));
            if (element != null)
            {
                IWebElement closeButton = _driver.FindElement(By.XPath("//div[@class='box_x_button']"));
                closeButton.Click();
            }
        }
        catch (Exception) { }
    }

}

