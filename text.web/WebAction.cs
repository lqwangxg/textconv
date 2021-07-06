using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Text.Common;
namespace Text.Web
{
    public class WebAction
    {
        public string name;
        public int interval;
        public int shotfromstep;
        public List<WebActionItem> actions = new List<WebActionItem>();
        public List<string> shotcmd = new List<string>();
        
        public int nextStepIndex { get; private set; }
        
        public IDictionary<string, object> vars { get; private set; }

        public IWebDriver driver;
        private IJavaScriptExecutor js;
        public WebAction()
        {
            vars = new Dictionary<string, object>();
        }
        public void Init()
        {
            foreach (WebActionItem item in actions)
            {
                item.parent = this;
                item.Init();
            }
        }
        public void wait()
        {
            wait(interval);
        }
        public void wait(int timeout)
        {
            try
            {
                Thread.Sleep(timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }

        public bool doAction(WebActionItem action)
        {
            int retryIndex = 0;
            int maxRetry = int.Parse(Config.GetAppSettingValue2("web.error.retry", "3"));
            
            while (retryIndex < maxRetry)
            {
                try
                {
                    windowAction(action);
                    wait();
                    return true;
                }
                catch (WebDriverException ex)
                {
                    retryIndex++;
                    Console.WriteLine(ex.Message);
                    continue;
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return false;
        }

        private bool windowAction(WebActionItem action)
        {
            // open url
            if (action.IsCmd("open"))
            {
                driver.Navigate().GoToUrl(action.target);
                return true;
            }
            if (action.IsCmd("runjs"))
            {
                js = (IJavaScriptExecutor)driver;
                if (!string.IsNullOrEmpty(action.target))
                {
                    var element = FindElement(action);
                    js.ExecuteScript(action.value, element);
                }
                else
                {
                    js.ExecuteScript(action.value);
                }
                
            }
            // switch to dest window or frame
            if (action.IsTarget("switchwindow"))
            {
                driver.SwitchTo().Window(vars[action.value].ToString());
                return true;
            }

            if (action.IsCmd("switchTo"))
            {
                if (action.IsTarget("window"))
                {
                    driver.SwitchTo().Window(vars[action.value].ToString());
                }
                if (action.IsTarget("Frame"))
                {
                    if (action.index != -1)
                    {
                        driver.SwitchTo().Frame(action.index);
                    }else
                    {
                        driver.SwitchTo().Frame(action.value);
                    }
                }
                else if (action.IsTarget("DefaultContent"))
                {
                    driver.SwitchTo().DefaultContent();
                }
                return true;
            }

            // resize, wait or others
            if (otherAction(action))return true;

            // find the element, click or sendkeys
            if (elementAction(action)) return true;

            return false;
        }
        private bool otherAction(WebActionItem action)
        {
            // resize window
            if (action.IsCmd("resize"))
            {
                if (action.IsTarget("window"))
                {
                    driver.Manage().Window.Size = action.size;
                }
                return true;

            }
            // wait interval ms.
            if (action.IsCmd("wait"))
            {
                if (string.IsNullOrEmpty(action.value))
                {
                    wait();
                }
                else
                {
                    wait(int.Parse(action.value));
                }
                return true;
            }
            // save WindowHandles
            if (action.IsCmd("save"))
            {
                if (action.IsTarget("WindowHandles"))
                {
                    vars[action.value] = driver.WindowHandles;
                }else if (action.IsTarget("CurrentWindowHandle"))
                {
                    vars[action.value] = driver.CurrentWindowHandle;
                }
                return true;
            }
            // save WindowHandles
            if (action.IsCmd("close"))
            {
                driver.Close();
                return true;
            }
            // save WindowHandles
            if (action.IsCmd("sendkeys"))
            {
                if(string.IsNullOrEmpty(action.target) 
                || Regex.IsMatch(action.target, "window"))
                {
                    SendKeys.SendWait(action.value);
                    return true;
                }
            }
            // save WindowHandles
            if (action.IsCmd("waitForWindow"))
            {
                if (!string.IsNullOrEmpty(action.target))
                {
                    vars[action.target] = waitForWindow(int.Parse(action.value));
                }
                return true;
            }
            if (action.IsCmd("var"))
            {
                vars[action.target] = action.value;
                return true;
            }
            if (action.IsCmd("math"))
            {
                mathAction(action);
                return true;
            }

            if (action.IsCmd("ifvar"))
            {
                // if not equals, skip goto.
                action.isTrue =action.IsTargetValid(vars);
                return true;
            }
            return false;
        }
        private void mathAction(WebActionItem action)
        {
            //Add substract
            Match m = Regex.Match(action.target, @"(\w+)([\+\-\*\/])(\d+)");
            if (m.Success)
            {
                string cmd = m.Groups[1].Value;
                string sign = m.Groups[2].Value;
                string val2 = m.Groups[3].Value;
                decimal var1 = 0;
                if (vars.ContainsKey(cmd))
                {
                    var1 = decimal.Parse(vars[cmd].ToString());
                }
                else
                {
                    vars.Add(cmd, var1);
                }

                if (sign.Equals("+"))
                {
                    var1 = var1 + decimal.Parse(val2);
                }
                else if (sign.Equals("-"))
                {
                    var1 = var1 - decimal.Parse(val2);
                }
                else if (sign.Equals("*"))
                {
                    var1 = var1 * decimal.Parse(val2);
                }
                else if (sign.Equals("/"))
                {
                    var1 = var1 / decimal.Parse(val2);
                }
                vars[cmd] = var1;
            }
        }
        private string waitForWindow(int timeout)
        {
            try
            {
                Thread.Sleep(timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
            var whNow = ((IReadOnlyCollection<object>)driver.WindowHandles).ToList();
            var whThen = ((IReadOnlyCollection<object>)vars["WindowHandles"]).ToList();
            if (whNow.Count > whThen.Count)
            {
                return whNow.Except(whThen).First().ToString();
            }
            else
            {
                return whNow.First().ToString();
            }
        }
        
        private bool elementAction(WebActionItem action)
        {
            IWebElement element = TryFindElement(action);

            // if not found, goto
            if (action.IsCmd("ifnot"))
            {
                action.isTrue = (element == null);
                return true;
            }
            // if not found, goto
            if (action.IsCmd("ifind"))
            {
                action.isTrue = (element != null);
                return true;
            }

            if (element == null)
            {
                return false;
            }
            if (action.IsCmd("mouseover"))
            {
                Actions builder = new Actions(driver);
                builder.MoveToElement(element).Perform();
                return true;
            }
            if (action.IsCmd("mouseout"))
            {
                Actions builder = new Actions(driver);
                builder.MoveToElement(element, 0, 0).Perform();
            }

            if (action.IsCmd("click"))
            {
                element.Click();
                return true;
            }
            if (action.IsCmd("popup"))
            {
                //POPUP画面起動前
                vars["WindowHandles"] = driver.WindowHandles;
                element.Click();
                wait();

                string windowId = string.Format("window{0}", DateTime.Now.ToString("yyyyMMddHHmmss"));
                //POPUP画面起動後
                vars[windowId] = waitForWindow(2000);
                //POPUP画面へ切替前、本画面を一時退避
                vars["root"] = driver.CurrentWindowHandle;
                //POPUP画面へ切替
                driver.SwitchTo().Window(vars[windowId].ToString());
                return true;
            }

            if (action.IsCmd("sendkeys") || action.IsCmd("type"))
            {
                element.SendKeys(action.value);
                return true;
            }else if (action.IsCmd("overwrite"))
            {
                element.Clear();
                if (!string.IsNullOrEmpty(action.value))
                {
                    element.SendKeys(action.value);
                }
                return true;
            }
            return false;
        }
        
        private IWebElement TryFindElement(WebActionItem action)
        {
            IWebElement element = null;
            int frameIndex = 0;
            IWebElement brotherFrame = null;
            IReadOnlyCollection<IWebElement> frameset = null;

            int retry = 0;

            // if not found in current page, check frame.
            while (retry < 2)
            {
                try
                {
                    element = FindElement(action);
                    return element;
                }
                catch(WebDriverException ex)
                {
                    bool isKeepAlive = true;
                    if(ex.InnerException != null)
                    {
                        System.Net.WebException wex = ex.InnerException as System.Net.WebException;
                        if(wex.Status == System.Net.WebExceptionStatus.KeepAliveFailure)
                        {
                            isKeepAlive = false;
                            retry++;
                        }
                    }
                    if (isKeepAlive)
                    {
                        if (brotherFrame == null)
                        {
                            frameIndex = 0;
                            frameset = driver.FindElements(By.XPath("//frame"));
                            if (frameset.Count > 0)
                            {
                                driver.SwitchTo().Frame(frameIndex);
                                brotherFrame = frameset.ToArray()[frameIndex];
                                frameIndex++;
                            }
                            else
                            {
                                if (!action.IsCmd("ifnot") &&  !action.IsCmd("ifind"))
                                {
                                    Console.WriteLine("★Error on {0}\n{1}", action.ToString(), ex.Message);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        else if (frameset.Count > frameIndex)
                        {
                            driver.SwitchTo().ParentFrame();
                            driver.SwitchTo().Frame(frameIndex);
                            brotherFrame = frameset.ToArray()[frameIndex];
                            frameIndex++;
                        }
                        else
                        {
                            brotherFrame = null;
                        }
                        retry++;
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return null;
        }
        private IWebElement FindElement(WebActionItem action)
        {
            IWebElement element = null;
            if (action.IsTargetKey("css"))
            {
                element = driver.FindElement(By.CssSelector(action.targetPair.value));
            }
            if (action.IsTargetKey("id"))
            {
                element = driver.FindElement(By.Id(action.targetPair.value));
            }
            if (action.IsTargetKey("name"))
            {
                element = driver.FindElement(By.Name(action.targetPair.value));
            }
            if (action.IsTargetKey("linktext"))
            {
                element = driver.FindElement(By.LinkText(action.targetPair.value));
            }
            if (action.IsTargetKey("XPath"))
            {
                if (string.IsNullOrEmpty(action.value))
                {
                    element = driver.FindElement(By.XPath(action.targetPair.value));
                }
                else if (action.IsValueKey("parentname"))
                {
                    var parent = driver.FindElement(By.Name(action.valuePair.value));
                    element = parent.FindElement(By.XPath(action.targetPair.value));
                }
            }
            else if (action.IsTargetKey("XPathFormat")) 
            {
                string xpath = string.Empty;
                if (action.IsValuePair)
                {
                    xpath = string.Format(action.targetPair.value, action.valuePair.value);
                }
                else
                {
                    xpath = string.Format(action.targetPair.value, action.value);
                }
                if (!string.IsNullOrEmpty(xpath))
                {
                    element = driver.FindElement(By.XPath(xpath));
                }
            }

            return element;
        }
    }

}
