# Current Version 1.0
https://github.com/Russell-KV4S/IRLP.StationTracking/releases/download/1.0/IRLP.StationTracking.zip

# IRLP.StationTracking
IRLP.StationTracking gives you ability to get email notificaitons about status changes of your Favorite IRLP stations.
The program reads data from this site: http://status.irlp.net/index.php?PSTART=9

Contact me if you have feature request or use Git and create your enhancements and merge them back in.

Once you download, edit the .config file that's along side the executable as needed (you won't need to copy the config on future releases unless there is a structure change). 
There are comments in the file that tells you how to format the entries. Here is the example file:
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2" />
    </startup>
    <appSettings>
        <!--use commas with no spaces to add more-->
        <add key="Callsigns" value="KV4S"/>
        <!--"Y" or "N" values-->
        <!--If you run this as a job or don't need to see the output then make Unattended Yes-->
        <add key="Unattended" value="N"/>
        <add key="EmailError" value="Y"/>
        <add key="StatusEmails" value="Y"/>
      
      <!--Email Parameters - Gmail example-->
      <!--use commas with no spaces to add more emails to the email To and From field-->
      <add key="EmailTo" value="example@gmail.com"/>
      <add key="EmailFrom" value="example@gmail.com"/>
      <add key="SMTPHost" value="smtp.gmail.com"/>
      <add key="SMTPPort" value="587"/>
      <add key="SMTPUser" value="example@gmail.com"/>
      <add key="SMTPPassword" value="Password"/>
    </appSettings>
</configuration>
```
