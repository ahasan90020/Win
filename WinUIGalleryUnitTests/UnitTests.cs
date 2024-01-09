using AppUIBasics;
using AppUIBasics.Data;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Diagnostics;

namespace WinUIGalleryUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestControlInfoDataSource()
        {
            Assert.AreEqual(0, 0);

            var groups = await ControlInfoDataSource.Instance.GetGroupsAsync();
            var groupsList = groups.ToList();

            var expectedGroups = new List<string>
            {
                "Design guidance",
                "Accessibility",
                "Menus & toolbars",
                "Collections",
                "Date & time",
                "Basic input",
                "Status & info",
                "Dialogs & flyouts",
                "Scrolling",
                "Layout",
                "Navigation",
                "Media",
                "Styles",
                "Text",
                "Motion",
                "Windowing",
                "System"
            };

            Assert.AreEqual(expectedGroups.Count, groupsList.Count);

            int groupCount = expectedGroups.Count;
            for(int i = 0; i < groupCount; i++)
            {
                var actualTitle = groupsList[i].Title;
                Assert.AreEqual(expectedGroups[i], actualTitle);
            }
        }

        // Use the UITestMethod attribute for tests that need to run on the UI thread.
        [UITestMethod]
        public void TestWrapGrid()
        {
            AppUIBasics.WrapPanel wrapPanel = new AppUIBasics.WrapPanel();
            wrapPanel.Width = 250;
            wrapPanel.Height = 250;
            for (int i = 0; i < 4; i++) 
            {
                Button button = new Button
                {
                    Width = 120,
                    Height = 80,
                    Content = $"Button {i}"
                };
                wrapPanel.Children.Add(button);
            }

            UnitTestApp.UnitTestAppWindow.AddToVisualTree(wrapPanel);
            wrapPanel.UpdateLayout();

            List<Rect> expectedLayouts = new List<Rect>
            {
                new Rect(0,    0, 120,  80),
                new Rect(120,  0, 120,  80),
                new Rect(0,   80, 120,  80),
                new Rect(120, 80, 120,  80)
            };
            for (int i = 0; i < 4; i++)
            {
                var actualLayout = LayoutInformation.GetLayoutSlot(wrapPanel.Children[i] as FrameworkElement);
                Assert.AreEqual(expectedLayouts[i], actualLayout);
            }
        }

        [TestMethod]
        public void Test2()
        {
            Border border = null;
            AutoResetEvent borderSizeChanged = new AutoResetEvent(false);

            ExecuteOnUIThread(() =>
            {
                ControlExample example = new ControlExample();
                example.Example = new TextBlock {  Text = "Test" };


                Grid grid = new Grid
                {
                    Width = 200,
                };

                border = new Border
                {
                    Background = new SolidColorBrush(Colors.Green),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Child = new Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.Red),
                        Width = 100,
                    }
                };

                border.SizeChanged += (s, e) =>
                {
                    borderSizeChanged.Set();
                };

                grid.Children.Add(border);
                
                UnitTestApp.UnitTestAppWindow.AddToVisualTree(grid);
                UnitTestApp.UnitTestAppWindow.AddToVisualTree(example);
            });
            Thread.Sleep(5000);

            Assert.IsTrue(borderSizeChanged.WaitOne());

            ExecuteOnUIThread(() =>
            {
                Assert.AreEqual(200, border.ActualWidth);

                border.HorizontalAlignment = HorizontalAlignment.Left;
            });

            Assert.IsTrue(borderSizeChanged.WaitOne());

            ExecuteOnUIThread(() =>
            {
                Assert.AreEqual(100, border.ActualWidth);
            });
        }

        private void ExecuteOnUIThread(Action action)
        {
            AutoResetEvent done = new AutoResetEvent(false);
            DispatcherQueue dispatcherQueue = UnitTestApp.UnitTestAppWindow.DispatcherQueue;
            if (dispatcherQueue.HasThreadAccess)
            {
                action();
            }
            else
            {
                Exception exception = null;
                var success = dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex) 
                    { 
                        exception = ex;
                    }
                    finally 
                    { 
                        done.Set(); 
                    }
                });
                Assert.IsTrue(success);
                Assert.IsTrue(done.WaitOne());
                if(exception != null)
                {
                    Assert.Fail(exception.ToString());
                }
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if(UnitTestApp.UnitTestAppWindow.DispatcherQueue.HasThreadAccess)
            {
                UnitTestApp.UnitTestAppWindow.CleanupVisualTree();
            }
            else
            {
                ExecuteOnUIThread(() =>
                {
                    UnitTestApp.UnitTestAppWindow.CleanupVisualTree();
                });
            }
        }
    }
}
