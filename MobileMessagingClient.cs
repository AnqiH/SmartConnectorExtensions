using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;


public class MobileMessagingClient : IMobileMessagingClient
{
    private readonly FirebaseMessaging messaging;

    public MobileMessagingClient()
    {
        var app = FirebaseApp
            .Create(new AppOptions() {
                Credential = GoogleCredential.FromFile("serviceAccountKey.json")
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging") 
            });

        messaging = FirebaseMessaging.GetMessaging(app);
    }
        
}