# Discord Blog Feed

Blog posts are written in a **private Discord channel**. The website reads that channel with a bot, saves each post in the
database (`BlogPosts` table) and shows them on the Blog page. Nobody needs a login on the website to write a post. Anyone
who can type in the private channel can publish.

## How posting works

In the private channel, start the message with `Title:` on the first line. The rest of the message is the post body:

```
Title: Version 1.1 is out!
We fixed the bugs you reported and added a new map.
Thanks for playing!
```

- The first line must start with `Title:` (capital letters do not matter). Messages that do not start this way are ignored, so the channel can also be used for normal chat.
- The post date is the day the message was sent (UTC). The author is the Discord display name of the sender.
- **Edit the message** in Discord and the post updates on the site within about 5 minutes.
- **Delete the message**, or edit it so it no longer starts with `Title:`, and the post is removed from the site within about 5 minutes. **Its comments are removed with it.**
- The site checks Discord at most once every 5 minutes (when someone opens the Blog page). If Discord is down, the saved posts keep showing and it retries after 1 minute.
- Only the newest 100 messages in the channel are read. Older posts stay in the database.
- Text only. Images and attachments are ignored (Discord image links expire). Discord formatting such as `**bold**` shows as typed.
- Line breaks in the message are kept.

## 1. Create the bot (once, by someone with Manage Server permission on the Discord server)

1. Open the [Discord Developer Portal](https://discord.com/developers/applications) and sign in. Click **New Application** and name it (for example "DCS Blog").
2. Open the **Bot** tab.
3. Click **Reset Token** and copy the token. **Treat it like a password.** You will not be able to see it again.
4. On the same tab, under **Privileged Gateway Intents**, turn on **Message Content Intent**. Without this, Discord sends every message empty and nothing will be published.
5. Open **OAuth2 > URL Generator**. Tick the scope **bot**. Under bot permissions tick **View Channels** and **Read Message History** (nothing else).
6. Open the generated URL, choose your server and click Authorize. The bot now appears in the member list.
7. The channel is private, so give the bot access to it: right-click the channel > **Edit Channel > Permissions**, add the bot (or its role) and allow **View Channel** and **Read Message History**.
8. Get the channel id: in Discord go to **User Settings > Advanced** and turn on **Developer Mode**. Then right-click the channel > **Copy Channel ID**.

## 2. Give the site the token and channel id

Never commit these. On your computer, right-click the **DCSProject** project > **Manage User Secrets** and add a `Discord` section next to the other sections:

```json
"Discord": {
  "BotToken": "paste-the-bot-token-here",
  "ChannelId": "paste-the-channel-id-here"
}
```

On Azure, add app settings (two underscores replace the colon): `Discord__BotToken` and `Discord__ChannelId`.

If these are not set the Discord feed is simply off and the Blog page shows whatever posts are already in the database.

## 3. Database

This feature adds one column to `BlogPosts` (`DiscordMessageId`, unique). In the Package Manager Console run:

```
Add-Migration AddDiscordMessageId
Update-Database
```

Commit the new files in the `Migrations` folder.

## 4. Security notes

- **The bot token is a password.** If it is ever exposed (posted in chat, committed to GitHub), go back to the Developer Portal > Bot > **Reset Token** and update the secret.
- Give the bot only **View Channels** and **Read Message History**. It never writes to Discord.
- **Whoever can type in the private channel can publish to the website.** Keep the channel limited to the people who should be allowed to post.
- Post text is HTML-encoded when it is shown, so a message cannot add scripts or HTML to the page.
- If the bot ever reads zero messages, or only empty messages, the site changes nothing. It never deletes posts because of a Discord problem.

## 5. Troubleshooting

Check the Output window (or console) for these messages when the Blog page loads:

| Message | Meaning and fix |
|---|---|
| `Discord returned 401` | Wrong bot token. Copy it again (or reset it) and update the secret. |
| `Discord returned 403` | The bot cannot see the channel. Redo step 7 in section 1. |
| `Discord returned 404` | Wrong channel id, or the bot was not added to the server. |
| `Discord returned 429` | Asked too often. It retries automatically. |
| `Every Discord message came back empty` | Message Content Intent is off. Redo step 4 in section 1. |
| `Discord returned no messages` | The bot is missing **Read Message History** on the channel. |
| No message at all and no posts | The `Discord` secrets are missing, so the feed is off. |
| Posts do not appear | The message must start with `Title:` on the first line, and it can take up to 5 minutes. |

## 6. Testing checklist

1. Add the bot, the intent and the secrets, and run `Add-Migration` and `Update-Database`.
2. Post `Title: Test post` plus a body line in the channel. Open the Blog page. The post should appear.
3. Post a message without `Title:`. It should not appear.
4. Edit the test post in Discord and wait up to 5 minutes. The Blog page should show the change.
5. Delete the test post. After up to 5 minutes it should disappear from the Blog page and the `BlogPosts` table.
6. Remove the bot token from the secrets and reload. The page should still show saved posts and not crash.
