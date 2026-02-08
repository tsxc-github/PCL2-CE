Imports PCL.Core.MZMC.Yggdrasil

Public Class PageLoginMZMC
    Private Sub BtnBack_Click(sender As Object, e As EventArgs) Handles BtnBack.Click
        RunInUi(Sub() FrmLaunchLeft.RefreshPage(True))
    End Sub
    Private Sub BtnLogin_Click(sender As Object, e As EventArgs) Handles BtnLogin.Click
        BtnLogin.IsEnabled = False
        BtnBack.IsEnabled = False

        Dim LoginData As New McLoginMZMC(McLoginType.MZMC) With {
                .BaseUrl = If(Yggdrasil.ServerUrl.EndsWithF("/"),
                              Yggdrasil.ServerUrl & "authserver",
                              Yggdrasil.ServerUrl & "/authserver"),
                .Description = "Authlib-Injector",
                .Type = McLoginType.MZMC}

        Dispatcher.BeginInvoke(Async Function() As Task
            Try
                IsCreatingProfile = True
                LoginData.Token = Yggdrasil.GetToken()
                McLoginMZMCLoader.Start(LoginData, IsForceRestart:=True)
                Do While McLoginMZMCLoader.State = LoadState.Loading
                    BtnLogin.Text = Math.Round(McLoginMZMCLoader.Progress * 100) & "%"
                    Await Task.Delay(50)
                Loop
                If McLoginMZMCLoader.State = LoadState.Finished Then
                    FrmLaunchLeft.RefreshPage(True)
                ElseIf McLoginMZMCLoader.State = LoadState.Aborted Then
                    Hint("已取消登录！")
                ElseIf McLoginMZMCLoader.Error Is Nothing Then
                    Throw New Exception("未知错误！")
                Else
                    Throw New Exception(McLoginMZMCLoader.Error.Message, McLoginMZMCLoader.Error)
                End If
            Catch ex As Exception
                If ex.Message = "$$" Then
                ElseIf ex.Message.StartsWith("$") Then
                    Hint(ex.Message.TrimStart("$"), HintType.Critical)
                Else
                    Log(ex, "第三方登录尝试失败", LogLevel.Msgbox)
                End If
            Finally
                IsCreatingProfile = False
                BtnLogin.IsEnabled = True
                BtnBack.IsEnabled = True
                BtnLogin.Text = "登录"
            End Try
        End Function)

    End Sub
End Class
