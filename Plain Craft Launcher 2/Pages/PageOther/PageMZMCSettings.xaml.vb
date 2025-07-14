Imports PCL.Core.MZMC.API.SDK
Imports PCL.Core.MZMC.Init

Public Class PageMZMCSettings

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not Me.IsLoaded Then Exit Sub
        Update_UserInfo()
    End Sub

    Private Sub Update_UserInfo()
        TextBlockUserName.Text = PCL.Core.MZMC.Init.User.Username
        TextBlockIfUserLogin.Text = If(PCL.Core.MZMC.Init.User.IfLogin().OK, "已登录", "未登录")

    End Sub

    Private _isRunning As Boolean = False
    Private Sub LoginButton_Click(sender As Object, e As RoutedEventArgs) Handles ButtonLogin.Click

        'Dim Converter As New MyMsgBoxConverter With {.ForceWait = True, .Type = MyMsgBoxType.MZMCLogin}
        'WaitingMyMsgBox.Add(Converter)
        OpenWebsite(OAuthUrl)

        If _isRunning Then Return

        Dim listener = New HttpListener()
        listener.Prefixes.Add(RedirectUrl)
        listener.Start()
        _isRunning = True
        listener.GetContextAsync.ContinueWith(Sub(task)
                                                  Dim code = task.Result.Request.QueryString.Get("code")
                                                  task.Result.Response.ContentType = "text/plain;charset=UTF-8"
                                                  Using stream = task.Result.Response.OutputStream
                                                      stream.Write(Encoding.UTF8.GetBytes("登录成功"), 0, Encoding.UTF8.GetBytes("登录成功").Length)
                                                  End Using
                                                  listener.Stop()
                                                  _isRunning = False
                                                      Dispatcher.Invoke(Sub()
                                                                            PCL.Core.MZMC.Init.User.OAuthLogin(code)
                                                                            Update_UserInfo()
                                                                        End Sub)

                                              End Sub)
    End Sub

    Private Sub LogoutButton_Click(sender As Object, e As RoutedEventArgs) Handles ButtonLogout.Click
        PCL.Core.MZMC.Init.User.Logout()
        Update_UserInfo()
    End Sub

End Class
