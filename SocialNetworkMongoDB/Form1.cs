using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SocialNetworkMongoDB.MongoBDCollections;
using System.Linq;
using System.Xml.Linq;

namespace SocialNetworkMongoDB;

public partial class FormMain : Form
{
    public FormMain()
    {
        InitializeComponent();
        InitDB();
        tabControlAll.SelectedIndex = 2;
    }

    private MongoClient? client;
    private IMongoDatabase? database;
    private IMongoCollection<User>? usersCollection;
    private User? currentUser;
    private User? profileUser;
    private User? postsUser;
    private User[]? foundPeople;
    private User[]? friends;
    private Post? currentPost;
    private Post[]? allPosts;
    private Dictionary<string, string>? postToUserId;
    private bool? addingCommentFromUserPage = null;

    private void InitDB()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI");
        Console.WriteLine($"connection string: {connectionString}");

        if (connectionString is null)
        {
            Console.WriteLine("You must set your 'MONGODB_URI' environment variable. To learn how to set it, see https://www.mongodb.com/docs/drivers/csharp/current/quick-start/#set-your-connection-string");
            Environment.Exit(0);
        }

        client = new MongoClient(connectionString);
        database = client.GetDatabase("SocialNetworkMongoDB");
        usersCollection = database.GetCollection<User>("users");
    }

    private void buttonLogIn_Click(object sender, EventArgs e)
    {
        if (textBoxLogInEmail.Text.Length == 0)
        {
            textBoxLogInEmail.ForeColor = Color.Red;
            textBoxLogInEmail.Text = "Enter an email!";
        }

        if (textBoxLogInPassword.Text.Length == 0)
        {
            textBoxLogInPassword.ForeColor = Color.Red;
            textBoxLogInPassword.Text = "Enter a password!";
        }

        if (textBoxLogInEmail.ForeColor == Color.Red || textBoxLogInPassword.ForeColor == Color.Red)
            return;

        var foundUser = usersCollection.Find(u => u.Email == textBoxLogInEmail.Text).FirstOrDefault();

        if (foundUser is null)
        {
            textBoxLogInEmail.ForeColor = Color.Red;
            textBoxLogInEmail.Text = "Enter a valid email!";
            return;
        }

        if (foundUser.Password != textBoxLogInPassword.Text)
        {
            textBoxLogInPassword.ForeColor = Color.Red;
            textBoxLogInPassword.Text = "Wrong password!";
            return;
        }

        Console.WriteLine($"user {foundUser.Email} logs in successfully");
        currentUser = foundUser;

        reloadPeople();
        updatePosts();
        loadProfilePage();

        panelLogIn.Visible = false;
        panelMainPage.Visible = true;
    }

    private void textBoxEmail_Click(object sender, EventArgs e)
    {
        if (textBoxLogInEmail.ForeColor == Color.Red)
        {
            textBoxLogInEmail.ForeColor = SystemColors.WindowText;
            textBoxLogInEmail.Text = "";
        }
    }

    private void textBoxPassword_Click(object sender, EventArgs e)
    {
        if (textBoxLogInPassword.ForeColor == Color.Red)
        {
            textBoxLogInPassword.ForeColor = SystemColors.WindowText;
            textBoxLogInPassword.Text = "";
        }
    }

    private void buttonSignIn_Click(object sender, EventArgs e)
    {
        panelLogIn.Visible = false;
        panelSignIn.Visible = true;
    }

    private void buttonSignInBack_Click(object sender, EventArgs e)
    {
        panelSignIn.Visible = false;
        panelLogIn.Visible = true;
    }

    private void buttonCreateUser_Click(object sender, EventArgs e)
    {

        if (textBoxSignInEmail.Text.Length == 0)
        {
            textBoxSignInEmail.ForeColor = Color.Red;
            textBoxSignInEmail.Text = "Enter an email!";
        }

        if (textBoxSignInPassword.Text.Length == 0)
        {
            textBoxSignInPassword.ForeColor = Color.Red;
            textBoxSignInPassword.Text = "Enter a password!";
        }

        if (textBoxSignInFirstName.Text.Length == 0)
        {
            textBoxSignInFirstName.ForeColor = Color.Red;
            textBoxSignInFirstName.Text = "Enter a first name!";
        }

        if (textBoxSignInLastName.Text.Length == 0)
        {
            textBoxSignInLastName.ForeColor = Color.Red;
            textBoxSignInLastName.Text = "Enter a last name!";
        }

        if (textBoxSignInEmail.ForeColor == Color.Red || textBoxSignInPassword.ForeColor == Color.Red ||
            textBoxSignInFirstName.ForeColor == Color.Red || textBoxSignInLastName.ForeColor == Color.Red)
            return;

        var foundUser = usersCollection.Find(u => u.Email == textBoxSignInEmail.Text).FirstOrDefault();

        if (foundUser is not null)
        {
            textBoxSignInEmail.ForeColor = Color.Red;
            textBoxSignInEmail.Text = "This email is already taken!";
            return;
        }

        User createdUser = new()
        {
            Email = textBoxSignInEmail.Text,
            Password = textBoxSignInPassword.Text,
            FirstName = textBoxSignInFirstName.Text,
            LastName = textBoxSignInLastName.Text,
            Interests = new(),
            Friends = new(),
            Posts = new()
        };

        usersCollection!.InsertOne(createdUser);

        Console.WriteLine($"user {createdUser.Email} created successfully");
    }

    private void buttonMainPageBack_Click(object sender, EventArgs e) => returnToLogInPage();

    private void returnToLogInPage()
    {
        panelMainPage.Visible = false;
        panelLogIn.Visible = true;
    }

    private void textBoxSignInEmail_Click(object sender, EventArgs e)
    {
        if (textBoxSignInEmail.ForeColor == Color.Red)
        {
            textBoxSignInEmail.ForeColor = SystemColors.WindowText;
            textBoxSignInEmail.Text = "";
        }
    }

    private void textBoxSignInPassword_Click(object sender, EventArgs e)
    {
        if (textBoxSignInPassword.ForeColor == Color.Red)
        {
            textBoxSignInPassword.ForeColor = SystemColors.WindowText;
            textBoxSignInPassword.Text = "";
        }
    }

    private void textBoxSignInFirstName_Click(object sender, EventArgs e)
    {
        if (textBoxSignInFirstName.ForeColor == Color.Red)
        {
            textBoxSignInFirstName.ForeColor = SystemColors.WindowText;
            textBoxSignInFirstName.Text = "";
        }
    }

    private void textBoxSignInLastName_Click(object sender, EventArgs e)
    {
        if (textBoxSignInLastName.ForeColor == Color.Red)
        {
            textBoxSignInLastName.ForeColor = SystemColors.WindowText;
            textBoxSignInLastName.Text = "";
        }
    }

    private void textBoxFindFriends_Click(object sender, EventArgs e)
    {
        if (textBoxFindFriends.ForeColor == SystemColors.GrayText)
        {
            textBoxFindFriends.ForeColor = SystemColors.WindowText;
            textBoxFindFriends.Text = "";
        }
    }

    private void textBoxFindFriends_Leave(object sender, EventArgs e)
    {
        if (textBoxFindFriends.Text.Length == 0)
        {
            textBoxFindFriends.ForeColor = SystemColors.GrayText;
            textBoxFindFriends.Text = "Find Friends";
        }
    }

    private void textBoxFindFriends_TextChanged(object sender, EventArgs e) => reloadPeople();


    private void reloadPeople()
    {
        var users = usersCollection.Find(u => u.Id != currentUser!.Id).ToList();

        foundPeople = textBoxFindFriends.Text.Length == 0 ?
            users.ToArray() : users.Where(u => u.ToString().Contains(
                textBoxFindFriends.Text, StringComparison.OrdinalIgnoreCase)).ToArray();

        listBoxPeople.DataSource = foundPeople;

        friends = usersCollection.Find(u => currentUser!.Friends!.Contains(u.Id!)).ToList().ToArray();

        listBoxMyFriends.DataSource = friends;
    }

    private void listBoxPeople_SelectedIndexChanged(object sender, EventArgs e)
    {
        Console.WriteLine(listBoxPeople.SelectedIndex);

        if (listBoxPeople.SelectedIndex == -1)
            buttonAddDeleteFriend.Text = "";
        else
        {
            var friendId = foundPeople![listBoxPeople.SelectedIndex].Id!;
            buttonAddDeleteFriend.Text = currentUser!.Friends!.Contains(friendId) ?
                "Delete Friend" : "Add Friend";
        }
    }

    private void buttonAddDeleteFriend_Click(object sender, EventArgs e)
    {
        var idx = listBoxPeople.SelectedIndex;

        if (idx == -1)
            return;

        var friendId = foundPeople![idx].Id!;

        if (currentUser!.Friends!.Contains(friendId))
        {
            usersCollection!.UpdateOne(u => u.Id == currentUser.Id,
                Builders<User>.Update.Pull(u => u.Friends, friendId));
            usersCollection!.UpdateOne(u => u.Id == friendId,
                Builders<User>.Update.Pull(u => u.Friends, currentUser.Id));
            currentUser = usersCollection!.Find(u => u.Id == currentUser.Id).First();
        }
        else
        {
            usersCollection!.UpdateOne(u => u.Id == currentUser.Id,
                Builders<User>.Update.AddToSet(u => u.Friends, friendId));
            usersCollection!.UpdateOne(u => u.Id == friendId,
                Builders<User>.Update.AddToSet(u => u.Friends, currentUser.Id));
            currentUser = usersCollection!.Find(u => u.Id == currentUser.Id).First();
        }

        reloadPeople();
        listBoxPeople.SelectedIndex = idx;
    }

    private void buttonProfileDeleteUser_Click(object sender, EventArgs e)
    {
        usersCollection.DeleteOne(u => u.Id == currentUser!.Id);
        currentUser = usersCollection!.Find(u => u.Id == currentUser!.Id).First();
        returnToLogInPage();
    }

    private void tabControlAll_Selected(object sender, TabControlEventArgs e)
    {
        switch (tabControlAll.SelectedIndex)
        {
            case 0:
                reloadPeople();
                break;
            case 1:
                reloadAllPosts();
                break;
            case 2:
                loadProfilePage();
                break;
        }
    }

    private void reloadAllPosts()
    {
        var upis = usersCollection!
            .Aggregate()
            .Unwind<User, UserUnwound>(u => u.Posts)
            .Group(uu => uu,
            upi => new
            {
                UserId = upi.Key.Id,
                PostId = upi.Key.Posts!.Id,
                upi.Key.Posts!.Date
            }
            )
            .SortByDescending(upi => upi.Date)
            .ToList()
            .ToArray();

        postToUserId = upis.ToDictionary(x => x.PostId!, x => x.UserId!);

        foreach (var upi in upis)
            Console.WriteLine($"{upi.UserId} - {upi.PostId} - {upi.Date}");

        if (checkBoxOnlyFriends.Checked)
            upis = upis.Where(u => currentUser!.Friends!.Contains(u.UserId!)).ToArray();

        allPosts = upis.Select(
            x => usersCollection!
            .Find(u => u.Id == x.UserId)
            .Project(u => u.Posts!.Where(p => p.Id == x.PostId).First())
            .First()
            ).ToArray();

        if (checkBoxOnlyInterests.Checked)
            allPosts = allPosts!
                .Where(p => p.Categories!.Any(c => currentUser!.Interests!.Contains(c)))
                .ToArray();

        listBoxPostsPosts.DataSource = allPosts;
    }

    private void loadProfilePage()
    {
        textBoxProfileEmail.Text = currentUser!.Email;
        textBoxProfilePassword.Text = currentUser.Password;
        textBoxProfileFirstName.Text = currentUser.FirstName;
        textBoxProfileLastName.Text = currentUser.LastName;
        textBoxProfileInterests.Text = string.Join("\r\n", currentUser!.Interests!);
    }

    private void buttonProfileSave_Click(object sender, EventArgs e)
    {
        var interests = textBoxProfileInterests.Text.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        usersCollection.UpdateOne(u => u.Id == currentUser!.Id, Builders<User>.Update
            .Set(u => u.Password, textBoxProfilePassword.Text)
            .Set(u => u.FirstName, textBoxProfileFirstName.Text)
            .Set(u => u.LastName, textBoxProfileLastName.Text)
            .Set(u => u.Interests, interests)
            );
        currentUser = usersCollection!.Find(u => u.Id == currentUser!.Id).First();
    }

    private void buttonUserProfileBack_Click(object sender, EventArgs e)
    {
        updateAfterPosts();

        panelUserProfile.Visible = false;
        panelMainPage.Visible = true;
    }

    private void buttonMyProfile_Click(object sender, EventArgs e)
    {
        profileUser = currentUser;

        reloadProfilePanel();
        showSelectedPost();
        updateAfterProfilePage();

        panelMainPage.Visible = false;
        panelUserProfile.Visible = true;
    }

    private void reloadProfilePanel()
    {
        labelProfilePageFullName.Text = profileUser!.ToString();
        listBoxProfilePageInterests.DataSource = profileUser.Interests;
        listBoxProfilePagePosts.DataSource = profileUser.Posts;

        textBoxProfilePageContent.Text = "";
        listBoxProfilePageCategories.DataSource = null;
        buttonProfilePageLikes.Text = $"Likes: -1";
        buttonProfilePageLikes.ForeColor = SystemColors.WindowText;
        buttonProfilePageDislikes.Text = $"Dislikes: -1";
        buttonProfilePageDislikes.ForeColor = SystemColors.WindowText;
        labelProfilePageDate.Text = "Date";
        listBoxProfilePageComments.DataSource = null;
    }

    private void listBoxProfilePagePosts_SelectedIndexChanged(object sender, EventArgs e) => showSelectedPost();

    private void showSelectedPost()
    {
        var idx = listBoxProfilePagePosts.SelectedIndex;

        if (idx == -1)
            return;

        currentPost = usersCollection.Find(u => u.Id == profileUser!.Id).First().Posts![idx];

        textBoxProfilePageContent.Text = currentPost.Content;
        listBoxProfilePageCategories.DataSource = currentPost.Categories;
        buttonProfilePageLikes.Text = $"Likes: {currentPost.Likes!.Count}";
        buttonProfilePageDislikes.Text = $"Dislikes: {currentPost.Dislikes!.Count}";
        labelProfilePageDate.Text = currentPost.Date.ToString();
        listBoxProfilePageComments.DataSource = currentPost.Comments;

        buttonProfilePageLikes.ForeColor = currentUserLikedCurrentPost() ? Color.Blue : SystemColors.WindowText;

        buttonProfilePageDislikes.ForeColor = currentUserDislikedCurrentPost() ? Color.Blue : SystemColors.WindowText;
    }

    private bool currentUserLikedCurrentPost() => usersCollection!.Find(
            Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
            Builders<User>.Filter.ElemMatch(u => u.Posts,
            p => p.Id == currentPost!.Id && p.Likes!.Contains(currentUser!.Id!))).Any();

    private bool currentUserDislikedCurrentPost() => usersCollection!.Find(
        Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
        Builders<User>.Filter.ElemMatch(u => u.Posts,
        p => p.Id == currentPost!.Id && p.Dislikes!.Contains(currentUser!.Id!))).Any();

    private void buttonProfilePageDeletePost_Click(object sender, EventArgs e)
    {
        var idx = listBoxProfilePagePosts.SelectedIndex;

        if (idx == -1 || profileUser!.Id != currentUser!.Id)
            return;

        currentPost = usersCollection.Find(u => u.Id == profileUser!.Id).First().Posts![idx];

        usersCollection!.UpdateOne(u => u.Id == currentUser.Id,
            Builders<User>.Update.PullFilter(u => u.Posts, p => p.Id == currentPost.Id));
        profileUser = currentUser = usersCollection!.Find(u => u.Id == currentUser!.Id).First();
        currentPost = null;
        reloadProfilePanel();
        showSelectedPost();
    }

    private void panelUserProfile_VisibleChanged(object sender, EventArgs e)
    {
        if (profileUser is not null)
        {
            var isThisUser = profileUser!.Id == currentUser!.Id;
            buttonProfilePageAddPost.Visible = isThisUser;
            buttonProfilePageDeletePost.Visible = isThisUser;
        }
    }

    private void buttonProfilePageAddPost_Click(object sender, EventArgs e)
    {
        panelUserProfile.Visible = false;
        panelAddPost.Visible = true;
    }

    private void buttonAddPostBack_Click(object sender, EventArgs e)
    {
        panelAddPost.Visible = false;
        panelUserProfile.Visible = true;
    }

    private void buttonAddPostAddPost_Click(object sender, EventArgs e)
    {
        Post createdPost = new()
        {
            Content = textBoxAddPostContent.Text,
            Date = DateTime.Now,
            Categories = textBoxAddPostCategories.Text.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
            Likes = new(),
            Dislikes = new(),
            Comments = new()
        };

        usersCollection!.UpdateOne(u => u.Id == currentUser!.Id,
            Builders<User>.Update.AddToSet(u => u.Posts, createdPost));
        profileUser = currentUser = usersCollection!.Find(u => u.Id == currentUser!.Id).First();
        reloadAllPosts();
        reloadProfilePanel();
    }

    private void buttonProfilePageLikes_Click(object sender, EventArgs e)
    {
        if (currentPost is null)
            return;

        if (buttonProfilePageLikes.ForeColor == Color.Blue)
        {
            buttonProfilePageLikes.ForeColor = SystemColors.WindowText;
            deleteLike();
        }
        else
        {
            buttonProfilePageLikes.ForeColor = SystemColors.WindowText;
            deleteDislike();
            buttonProfilePageLikes.ForeColor = Color.Blue;
            addLike();
        }

        updateAfterProfilePage();
    }

    private void updateAfterProfilePage()
    {
        profileUser = usersCollection!.Find(u => u.Id == profileUser!.Id).First();
        if (currentUser!.Id == profileUser!.Id)
            currentUser = profileUser;

        var idx = listBoxProfilePagePosts.SelectedIndex;
        reloadProfilePanel();
        listBoxProfilePagePosts.SelectedIndex = idx;
        showSelectedPost();
    }

    private void deleteLike() =>
        usersCollection!.UpdateOne(
        Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
        Builders<User>.Filter.ElemMatch(u => u.Posts, p => p.Id == currentPost!.Id),
        Builders<User>.Update.Pull(u => u.Posts.FirstMatchingElement().Likes, currentUser!.Id));

    private void addLike() =>
        usersCollection!.UpdateOne(
        Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
        Builders<User>.Filter.ElemMatch(u => u.Posts, p => p.Id == currentPost!.Id),
        Builders<User>.Update.AddToSet(u => u.Posts.FirstMatchingElement().Likes, currentUser!.Id));

    private void buttonProfilePageDislikes_Click(object sender, EventArgs e)
    {
        if (currentPost is null)
            return;

        if (buttonProfilePageDislikes.ForeColor == Color.Blue)
        {
            buttonProfilePageLikes.ForeColor = SystemColors.WindowText;
            deleteDislike();
        }
        else
        {
            buttonProfilePageLikes.ForeColor = SystemColors.WindowText;
            deleteLike();
            buttonProfilePageLikes.ForeColor = Color.Blue;
            addDislike();
        }

        updateAfterProfilePage();
    }

    private void deleteDislike() =>
        usersCollection!.UpdateOne(
        Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
        Builders<User>.Filter.ElemMatch(u => u.Posts, p => p.Id == currentPost!.Id),
        Builders<User>.Update.Pull(u => u.Posts.FirstMatchingElement().Dislikes, currentUser!.Id));

    private void addDislike() =>
        usersCollection!.UpdateOne(
        Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
        Builders<User>.Filter.ElemMatch(u => u.Posts, p => p.Id == currentPost!.Id),
        Builders<User>.Update.AddToSet(u => u.Posts.FirstMatchingElement().Dislikes, currentUser!.Id));

    private void listBoxFriends_DoubleClick(object sender, EventArgs e)
    {
        if (listBoxPeople.SelectedIndex == -1)
            return;

        profileUser = foundPeople![listBoxPeople.SelectedIndex];
        reloadProfilePanel();
        showSelectedPost();

        panelMainPage.Visible = false;
        panelUserProfile.Visible = true;
    }

    private void listBoxPostsPosts_SelectedIndexChanged(object sender, EventArgs e) => loadCurrentPost();

    private void loadCurrentPost()
    {
        var idx = listBoxPostsPosts.SelectedIndex;

        if (idx == -1)
            return;

        currentPost = allPosts![idx];

        postsUser = usersCollection!.Find(u => u.Id == postToUserId![currentPost.Id]).First();

        textBoxPostsContent.Text = currentPost.Content;
        listBoxPostsCategories.DataSource = currentPost.Categories;
        buttonPostsLikes.Text = $"Likes: {currentPost.Likes!.Count}";
        buttonPostsDislikes.Text = $"Dislikes: {currentPost.Dislikes!.Count}";
        Console.WriteLine(currentPost.Date);
        labelPostsDate.Text = currentPost.Date.ToString();
        listBoxPostsComments.DataSource = currentPost.Comments;

        buttonPostsAuthor.Text = postsUser.ToString();

        buttonPostsLikes.ForeColor = currentUserLikedCurrentPost() ? Color.Blue : SystemColors.WindowText;

        buttonPostsDislikes.ForeColor = currentUserDislikedCurrentPost() ? Color.Blue : SystemColors.WindowText;
    }

    private void buttonPostsAuthor_Click(object sender, EventArgs e)
    {
        if (postsUser == null)
            return;

        profileUser = postsUser;

        reloadProfilePanel();
        showSelectedPost();

        panelMainPage.Visible = false;
        panelUserProfile.Visible = true;
    }

    private void buttonPostsLikes_Click(object sender, EventArgs e)
    {
        if (currentPost is null)
            return;

        if (buttonPostsLikes.ForeColor == Color.Blue)
        {
            buttonPostsLikes.ForeColor = SystemColors.WindowText;
            deleteLike();
        }
        else
        {
            buttonPostsDislikes.ForeColor = SystemColors.WindowText;
            deleteDislike();
            buttonPostsLikes.ForeColor = Color.Blue;
            addLike();
        }

        updateAfterPosts();
    }

    private void buttonPostsDislikes_Click(object sender, EventArgs e)
    {
        if (currentPost is null)
            return;

        if (buttonPostsDislikes.ForeColor == Color.Blue)
        {
            buttonPostsDislikes.ForeColor = SystemColors.WindowText;
            deleteDislike();
        }
        else
        {
            buttonPostsLikes.ForeColor = SystemColors.WindowText;
            deleteLike();
            buttonPostsDislikes.ForeColor = Color.Blue;
            addDislike();
        }

        updateAfterPosts();
    }

    private void updateAfterPosts()
    {
        if (postsUser is null)
            return;

        postsUser = usersCollection!.Find(u => u.Id == postsUser!.Id).First();
        if (currentUser!.Id == postsUser!.Id)
            currentUser = postsUser;

        updatePosts();
    }

    private void updatePosts()
    {
        var idx = listBoxPostsPosts.SelectedIndex;
        reloadAllPosts();
        reloadPostsTab();
        listBoxPostsPosts.SelectedIndex = idx;
        loadCurrentPost();
    }

    private void reloadPostsTab()
    {
        textBoxPostsContent.Text = "";
        listBoxPostsCategories.DataSource = null;
        buttonPostsLikes.Text = $"Likes: -1";
        buttonPostsLikes.ForeColor = SystemColors.WindowText;
        buttonPostsDislikes.Text = $"Dislikes: -1";
        buttonPostsDislikes.ForeColor = SystemColors.WindowText;
        labelPostsDate.Text = "Date";
        listBoxPostsComments.DataSource = null;

        buttonPostsAuthor.Text = "Author";
    }

    private void checkBoxOnlyFriends_CheckedChanged(object sender, EventArgs e) => reloadAllPosts();

    private void checkBoxOnlyInterests_CheckedChanged(object sender, EventArgs e) => reloadAllPosts();

    private void buttonDeleteFriend_Click(object sender, EventArgs e)
    {
        var idx = listBoxMyFriends.SelectedIndex;

        if (idx == -1)
            return;

        var friendId = friends![idx].Id;

        usersCollection!.UpdateOne(u => u.Id == currentUser!.Id,
            Builders<User>.Update.Pull(u => u.Friends, friendId));
        usersCollection!.UpdateOne(u => u.Id == friendId,
            Builders<User>.Update.Pull(u => u.Friends, currentUser!.Id));

        currentUser = usersCollection!.Find(u => u.Id == currentUser!.Id).First();

        reloadPeople();
    }

    private void buttonProfilePageAddComment_Click(object sender, EventArgs e)
    {
        panelUserProfile.Visible = false;
        panelAddComment.Visible = true;
        addingCommentFromUserPage = true;
        loadCommentPage();
    }

    private void buttonAddCommentBack_Click(object sender, EventArgs e)
    {
        panelAddComment.Visible = false;

        if (addingCommentFromUserPage == true)
            panelUserProfile.Visible = true;
        else
            panelMainPage.Visible = true;
    }

    private void loadCommentPage()
    {
        if (currentPost is not null)
            textBoxCommentPostContent.Text = currentPost.Content;
    }

    private void buttonCommentAddComment_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textBoxCommentContent.Text) || currentPost is null)
            return;

        Comment comment = new()
        {
            UserId = currentUser!.Id,
            Content = textBoxCommentContent.Text,
            Date = DateTime.Now
        };

        usersCollection!.UpdateOne(
            Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
            Builders<User>.Filter.ElemMatch(u => u.Posts, p => p.Id == currentPost!.Id),
            Builders<User>.Update.AddToSet(u => u.Posts.FirstMatchingElement().Comments, comment));

        if (addingCommentFromUserPage is null)
            Console.WriteLine("something is wrong");
        else if (addingCommentFromUserPage == true)
            updateAfterProfilePage();
        else
            updateAfterPosts();

        addingCommentFromUserPage = null;
    }

    private void listBoxProfilePageComments_DoubleClick(object sender, EventArgs e) => loadProfilePageCommentUser();

    private void loadProfilePageCommentUser()
    {
        var idx = listBoxProfilePageComments.SelectedIndex;

        if (idx == -1)
            return;


        if (listBoxProfilePageComments.DataSource is not List<Comment> comments || comments.Count == 0)
            return;

        profileUser = usersCollection!.Find(u => u.Id == comments[idx].UserId).First();
        reloadProfilePanel();
        showSelectedPost();

        panelMainPage.Visible = false;
        panelUserProfile.Visible = true;
    }

    private void loadPostsCommentUser()
    {
        var idx = listBoxPostsComments.SelectedIndex;

        if (idx == -1)
            return;


        if (listBoxPostsComments.DataSource is not List<Comment> comments || comments.Count == 0)
            return;

        profileUser = usersCollection!.Find(u => u.Id == comments[idx].UserId).First();
        reloadProfilePanel();
        showSelectedPost();

        panelMainPage.Visible = false;
        panelUserProfile.Visible = true;
    }

    private void buttonProfilePageDeleteComment_Click(object sender, EventArgs e)
    {
        var idx = listBoxProfilePageComments.SelectedIndex;

        if (idx == -1)
            return;

        if (listBoxProfilePageComments.DataSource is not List<Comment> comments || comments.Count == 0)
            return;

        var comment = comments[idx];

        if (comment.UserId != currentUser!.Id)
            return;

        usersCollection!.UpdateOne(
            Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
            Builders<User>.Filter.ElemMatch(u => u.Posts, p => p.Id == currentPost!.Id),
            Builders<User>.Update.PullFilter(u => u.Posts.FirstMatchingElement().Comments, c => c.Id == comment.Id));

        updateAfterProfilePage();
    }

    private void listBoxProfilePageComments_SelectedIndexChanged(object sender, EventArgs e)
    {
        var idx = listBoxProfilePageComments.SelectedIndex;

        if (idx == -1)
        {
            buttonCommentAuthor.Text = "Author";
        }
        else
        {
            if (listBoxProfilePageComments.DataSource is not List<Comment> comments || comments.Count == 0)
                return;

            var user = usersCollection!.Find(u => u.Id == comments[idx].UserId).First();

            buttonCommentAuthor.Text = user.ToString();
        }
    }

    private void buttonCommentAuthor_Click(object sender, EventArgs e) => loadProfilePageCommentUser();

    private void buttonPostsCommentAuthor_Click(object sender, EventArgs e) => loadPostsCommentUser();

    private void listBoxPostsComments_SelectedIndexChanged(object sender, EventArgs e)
    {
        var idx = listBoxPostsComments.SelectedIndex;

        if (idx == -1)
        {
            buttonPostsCommentAuthor.Text = "Author";
        }
        else
        {
            if (listBoxPostsComments.DataSource is not List<Comment> comments || comments.Count == 0)
                return;

            var user = usersCollection!.Find(u => u.Id == comments[idx].UserId).First();

            buttonPostsCommentAuthor.Text = user.ToString();
        }
    }

    private void listBoxPostsComments_DoubleClick(object sender, EventArgs e) => loadPostsCommentUser();

    private void buttonPostsAddComment_Click(object sender, EventArgs e)
    {
        panelMainPage.Visible = false;
        panelAddComment.Visible = true;
        addingCommentFromUserPage = false;
        loadCommentPage();
    }

    private void buttonPostsDeleteComment_Click(object sender, EventArgs e)
    {
        var idx = listBoxPostsComments.SelectedIndex;

        if (idx == -1)
            return;

        if (listBoxPostsComments.DataSource is not List<Comment> comments || comments.Count == 0)
            return;

        var comment = comments[idx];

        if (comment.UserId != currentUser!.Id)
            return;

        usersCollection!.UpdateOne(
            Builders<User>.Filter.Eq(u => u.Id, postToUserId![currentPost!.Id]) &
            Builders<User>.Filter.ElemMatch(u => u.Posts, p => p.Id == currentPost!.Id),
            Builders<User>.Update.PullFilter(u => u.Posts.FirstMatchingElement().Comments, c => c.Id == comment.Id));

        updateAfterPosts();
    }
}