using MusicCollaborationManager_BDD_Tests.Drivers;
using MusicCollaborationManager_BDD_Tests.PageObjects;
using MusicCollaborationManager_BDD_Tests.Shared;
using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Microsoft.AspDotNetCore.Mvc.RazorPages;

namespace MusicCollaborationManager_BDD_Tests.StepDefinitions
{
    public class TestUser
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
    }

    [Binding]
    public class UserLoginsStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly LoginPageObject _loginPage;
        private readonly HomePageObject _homePage;
        private IConfiguration Configuration { get; }

        public UserLoginsStepDefinitions(ScenarioContext context, BrowserDriver browserDriver)
        {
            _scenarioContext = context;
            _loginPage = new LoginPageObject(browserDriver.Current);
            _homePage = new HomePageObject(browserDriver.Current);
            
            IConfigurationBuilder builder = new ConfigurationBuilder().AddUserSecrets<UserLoginsStepDefinitions>();
            Configuration = builder.Build();
        }

        [Given(@"the following users exist")]
        public void GivenTheFollowingUsersExist(Table table)
        {
            IEnumerable<TestUser> users = table.CreateSet<TestUser>();
            _scenarioContext["Users"] = users;
        }

        [Given(@"the following users do not exist")]
        public void GivenTheFollowingUsersDoNotExist(Table table)
        {
            IEnumerable<TestUser> nonUsers = table.CreateSet<TestUser>();
            _scenarioContext["NonUsers"] = nonUsers;
         }

        [Given(@"I am a user with first name '([^']*)'"), When(@"I am a user with first name '([^']*)'")]
        public void GivenIAmAUserWithFirstName(string firstName)
        {
            IEnumerable<TestUser> users = (IEnumerable<TestUser>)_scenarioContext["Users"];
            TestUser u = users.Where(u => u.FirstName == firstName).FirstOrDefault();
            if (u == null)
            {
                // must have been selecting from non-users
                IEnumerable<TestUser> nonUsers = (IEnumerable<TestUser>)_scenarioContext["NonUsers"];
                u = nonUsers.Where(u => u.FirstName == firstName).FirstOrDefault();
            }
            _scenarioContext["CurrentUser"] = u;
        }

        [When(@"I login")]
        public void WhenILogin()
        {
            _loginPage.GoTo();
            TestUser u = (TestUser)_scenarioContext["CurrentUser"];
            _loginPage.EnterEmail(u.Email);
            _loginPage.EnterPassword(u.Password);
            _loginPage.Login();
        }

        [Then(@"I am redirected to the '([^']*)' page")]
        public void ThenIAmRedirectedToThePage(string pageName)
        {
            _loginPage.GetURL().Should().Be(Common.UrlFor(pageName));
        }

        [Then(@"I can see the '([^']*)' Button")]
        public void ThenICanSeeTheButton(string button)
        {
            _homePage.DashboardAnchor.Should().NotBeNull();
            _homePage.DashboardAnchor.Displayed.Should().BeTrue();
        }

        [Then(@"I can see a login error message")]
        public void ThenICanSeeALoginErrorMessage()
        {
            _loginPage.HasLoginErrors().Should().BeTrue();
        }

        //[Then(@"I can save cookies")]
        //public void ThenICanSaveCookies()
        //{
        //    throw new PendingStepException();
        //}

        //[Given(@"I am on the ""([^""]*)"" page")]
        //public void GivenIAmOnThePage(string home)
        //{
        //    throw new PendingStepException();
        //}

        //[When(@"I load previously saved cookies")]
        //public void WhenILoadPreviouslySavedCookies()
        //{
        //    throw new PendingStepException();
        //}

        //[When(@"I am on the ""([^""]*)"" page")]
        //public void WhenIAmOnThePage(string home)
        //{
        //    throw new PendingStepException();
        //}

        //[Then(@"I can see a personalized message in the navbar that includes my email")]
        //public void ThenICanSeeAPersonalizedMessageInTheNavbarThatIncludesMyEmail()
        //{
        //    throw new PendingStepException();
        //}
    }
}
