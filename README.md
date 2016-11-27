# NLogGwoKusExtensions
Extensions for NLog library.

## General
In order to use those custom targets you need to add this to your config file inside `<nlog>` element.
```xml
<extensions>
  <add assembly="NLogGwoKusExtensions"/>
</extensions>
```

# Slack Target
Target to send messages to slack channel using Slack Bot. Bot needs to be invited into channel.

## Parameters
**Required:**
* _SlackDomain_ - Your slack domain. https://**domain**.slack.com/
* _ApiKey_ - Your bot ApiKey. Found in bot preferences page. Format "xxxx-xxxxxxxx-xxxxxxxx-xxxxx".
* _Channel_ - Channel name without leading [#] or channel Id.

**Optional**
* _UserName_ - Bot username. This name will appear as bot name in chat.
* _Emoji_ - Emoji name with leading and trailing colon to set as bot avatar.
* _IconUrl_ - Icon URL to set as bot avatar. _Emoji_ must be not set for this to work.
* _ThrowExceptions_ - If set to `true` exceptions are thrown to NLog framework for logging. Default is `false`.

## Examples
Only required parameters:
```xml
<targets>
  <target type="SlackBotTarget" name="s" 
          SlackDomain="domain" 
          ApiKey="xxxx-xxxxxxxx-xxxxxxxx-xxxxx" 
          Channel="general" />
</targets>
```

All parameters:
Only required parameters:
```xml
<targets>
  <target type="SlackBotTarget" name="s" 
          SlackDomain="domain" 
          ApiKey="xxxx-xxxxxxxx-xxxxxxxx-xxxxx" 
          Channel="general"
          UserName="CustomUserName"
          Emoji=":warning:"
          IconUrl="Delete Emoji if you want to use it."
          ThrowExceptions="false|true" />
</targets>
```
