EXTERNAL getValue(a) // a = variableID number
EXTERNAL inventoryContains(item) // item is object name

VAR avatarPhone = false
VAR mozPhone = false
VAR bollywoodPhone = false
VAR bollywoodID = false
VAR yoYoPhone = false
VAR checkYoYoMsgs = false

== function getValue(a) ==
~ return true
== function inventoryContains(item) ==
~ return true
== function came_from(-> knot)
~ return (TURNS_SINCE(knot) == 0)

=== Instructions ===
Move cursor to the top to see your inventory. Left click to use on the scene.Right click to look.
->END
=== Intro ===

=PhoneCall
// Black screen. Phone rings Fingers answers
// They're talking about a hit at a crack house but disguising their language.
Who's calling at this time?
Fingers: "Yo, Fingers", I said
Boss: "Yo!? This isn't the time for yo, my friend."
Shit! Who else would it be?
Fingers: "What's happened?", I said.
Boss: "One of our pharmacies had an unsatisfied customer.", he said.
Fingers: "Who'd-"
Boss: "I need you to go down and handle the complaint. I'll drop you a pin."
Fingers: "Got it."
Boss: "There'll be someone there, to act as a guide, a local."
// Asking someone in the other room
Boss: "Hey, what's that girl's name again?"
He wasn't asking me.
Someone else where he was.
Boss: "The sketchy one who called you.", he asked the other person.
The reply was inaudiable.
// Returning to the call.
Boss: "She's called Nose, she did us a favour by relaying the complaint to us, but keep her in line."
Fingers: "Understood."
I heard him take a breath.
Boss: "The unsatisfied customer may have complained a lot, if you think the pharmacy needs to close, get the details of the incident, recover any stock and make sure nothing ties back to us, okay?"
Fingers: "That won't be a problem."
Boss: "Good."
He hung up.
Time to go to work.

-> END

=== Lobby ===

=Nose
The door was ajar. A woman stood waiting.
Fingers: "You Nose?" I asked.
Nose: "Yeah", she said.
Fingers: "You my guide?"
Nose: "Yeah."
She was gaunt, nervous and fidgety. Jonesing.
I introduced myself.
Fingers: "Fingers."
*   (knows) Fingers: "I see why they call you Nose."
    Nose: "Yeah, it's Knows, as in I knows stuff."
    Fingers: "Sure it is."
    Nose: "Why'd they call you Fingers, you a thief?"
    I wasn't sure if she was brave or stupid.
    Fingers: "Musician, actually."
    She wasn't sure how to respond.
    Nose: "Oh, right."
    She didn't believe me, and let me know.
    Not stupid then.
*   (live)Fingers: "You live round here?"
    Nose: "Yeah, top of the hill."
    Fingers: "You here often?"
    Nose: "Once or twice a week, trying to cut back."
    She fidgeted, she revealed more than she wanted too.
    I have that effect.
    Fingers: "Mmmm.", I grunted.
- Fingers: "Does anyone else know about this?"
Nose: "No, just us and One-Shot Jimmy."
Fingers: "One-Shot was here?"
Nose: "No, that's who I called."
Sometimes you get more answers by not asking.
Nose: "We had a thing back along."
Has she got anything else for me?
Nose: "He hasn't been here, what was I supposed to do? Call the police?"
Guess not.
*   Fingers: "Tell me what you know, what happened here?"
    Nose: "It's a mess, they're all dead, I don't know how."
    Fingers: "You recognise any of them"
    Nose: "Yeah, Moz is there and Avatar."
    The name Moz is familiar, small time drug dealer.
    Fingers: "Any product or money left out?"
    Nose: "No- I mean, I mean I didn't stick around to look, so I don't know."
    She figeted{live: again.}{not live: not a good liar.}
    Fingers: Mmmm, let's go in.
*   Fingers: Let's go in.

-Nose: You want me to stand watch?
Fingers: No, I want you in there.
// making fun of her
{knows:
I glare at her.
Fingers: Let's see what you 'knows.'
}

->END

=== LivingRoom ===

=EnterRoom
The boss has a talent for understatement.
A lot of complaining went on here.
Fingers: "Christ, someone's been busy", I said.
-> END

=Moz
-(opts)
* (look)[Look]
    Fingers: "Male, stabbed several times to the chest cavity. Probably bled out."
    There was a pistol on the floor.
    Fingers: "Has a piece. Glock 17, serial number filed off. That's a bit heavy for these jokers ain't it?"
    Nose: "I don't know, gun's a gun to me", Nose said.
    {Avatar.look:
    I looked at the two bodies.
    Fingers: "Reckon he was pinned to the chair and carved up by her, he got a couple off, and that's what did her."
    }
    -> opts
* (search) [Search]
    #sound Search, wait
    Fingers: "Nothing."
    -> opts
* (nose) [Ask Nose]
    #set KnowMoz = true
    Fingers: "Who's this?"
    Nose: "That's Moz, this is his gaff."
    Fingers: "He's your dealer?"
    Nose: "Yeah, he's alright though."
    She's telling the truth. Addiction and a bad pusher is not a good place for a young woman.
    {not Garden.Bollywood.look:
    Fingers: "He keep any heavies around?"
    Nose: "Yeah, big Paki fella, don't know his name. Never spoke to him."
    }
    -> opts
+ [Finish]
-> END

=Avatar
-(opts)
*(look) [Look]
    Fingers: "Female, shot twice, mid chest, quick death. Has a kitchen knife."
    {Moz.look: 
    I looked at the two bodies.
    "Gonna say she stuck him, and got plugged for her trouble."
    }
    -> opts
*(search) [Search]
    #sound Search, wait
    Fingers: "Nothing."
    -> opts
*(nose) [Ask Nose]
    #set KnowAvatar = true
    Fingers: "You know her?"
    Nose: "Yeah that's Avatar."
    Fingers: "Why'd they call her that?"
    Nose: "Because she's a big lanky black freaky bitch."
    I almost raised an eyebrow.
    Fingers: "What's she do, who is she?" I asked.
    Nose: "She's from round here, turns tricks down town, for anyone brave enough."
    My eyebrow twitched.
    Fingers: "Why? She riddled?"
    Nose: "No, I mean, who knows, but she's got a reputation for flipping on her clients and robbing 'em."
    Reasonable explaination. My eyebrow relaxed.
    Fingers: "Right."
    ->opts
+ [Finish]
-> END

=Lunch
-(opts)
*(look) [Look]
Fingers: "Male, missing head, looks like a shotgun close range. Still clutching a kitchen knife."
{Garden.Bollywood.look: 
Bollywood did this.
}
->opts
*(search) [Search]
#inventory add, LunchPhone
#sound Search, wait
#sound FindObj, wait
Fingers: "Phone in his pocket, face ID isn't going to work, 6 digit pin, hmm."
->opts
*(nose) [Ask Nose]
Fingers: "You recognise him?"
Nose: "No idea. Might know his face, if it was there."
Wonderful.
->opts
+ [Finish]
->END

=GetMozPhone
#sound GetPhone, wait
#inventory add, MozPhone
Fingers: "Hello Moz's phone." 
Face ID as well, so helpful.
-> END

=== Kitchen ===

=Phone
#inventory add, AvatarPhone
Fingers: "A phone on charge. Has face ID, Who do you belong to?"
#sound GetPhone
NoPlay:
-> END


=== Bathroom ===

=MedicineCabinet
{MedicineCabinet > 1:
#sound OpenCabinet, wait
Fingers: "Nothing else in here."
->END
}
#sound OpenCabinet, wait
I looked through the cabinet.
Fingers: "Co-codamol, strong, prescribed to Paige Harvester, you know her?"
Nose: "Nah", she said.
My eyes widened.
Fingers: "Wait a minute, what the-?"
Nose: "What?"
Fingers: "Precribed from Chapman Barracks, she's in the Army?"
Nose doesn't react.
{LivingRoom.Moz.look or Bedroom.YoYo.look or Garden.Bollywood.look:
Fingers: "That explains where the hardwear came from" I said.
}
->YoYoCause

=YoYoCause
{Bedroom.YoYo.look: 
#set KnowPaige = true
I put the medicine back.
Fingers: "Think she mixed her medicines and went too hard", I said. 
Fingers: "Probably nervous. She knew. That's why she's armed", I continue.
Fingers: "You reap what you sow."
Fingers: "Get it? 'Cause her name's Harvester."
//fake laughs
Nose: "Oh, yeah, hahaha, good one!"
Tough crowd.
Nose is eager to please, she wants a reward.
The boss will have something in mind.
I break the silence with a grunt.
Fingers: "Mmmm."
}
{Garden.Bollywood.search: 
Fingers: "Bollywood was Army too. Two soldiers in the same crack house, they knew each other." 
}
{came_from (->Bedroom.YoYo.look): 
->Bedroom.YoYo.opts 
}
->END

=== Bedroom ===

=YoYo
-(opts)
*(look) [Look]
Fingers: "Woman, no wounds, holding a pistol, Glock 17 interesting. Pale skin, blue lips, asphyxiated. No signs of struggle."
Nose: "She's OD'd."
Fingers: "How can you tell?"
Nose: "Seen it before."
Can't argue with that.
{Bathroom.MedicineCabinet:
->Bathroom.YoYoCause
}
->opts
*(search) [Search]
#sound Search, wait
Fingers: "Nothing."
->opts
*(nose) [Ask Nose]
Fingers: "You met her before?"
Nose: "Don't know, Moz seems to have a different woman everytime I come 'round."
->opts
+[Finish]
->END

=GetYoYoPhone
#sound GetPhone, wait
#inventory add, PaigePhone
Fingers: "Here you are, let's see who's face you recognise."
->END

=== Garden ===

=Bollywood
-(opts)
*(look) [Look]
#set KnowBollywood = true
Fingers: "Male, stab wounds to the back, blood trails from the house to here. Looks like he was trying to leave. Has a shotgun, Benelli M4, Army issue serial number filed off."
I'm only impressing myself with my knowledge of firearms.
Wait...
I know this man.
Fingers: "This is Bollywood, I've seen him around."
Nose: "Bollywood?"
Fingers: "He's Indian, and a flash fucker."
->opts
*(search) [Search]
#sound Search, wait 
#sound GetPhone, wait 
Fingers: "Phone's smashed. Probably fell hard on it. Useless now."
#sound FindObj, wait
#inventory add, IDCard
This is unexpected.
Fingers: "Army ID, Sergeant J Singh."
{Bathroom.MedicineCabinet: 
Fingers: "I reckon Sergeant Singh and pain killer Paige were connected."
}
-> opts
*(nose) [Ask Nose]
// If you ID'd Bollywood
{look: 
Fingers: "Did you know Bollywood?"
}
// If you haven't looked at him yet
{not look: 
Fingers: "Did you know him?"
}
Nose: "This is Moz's security, never spoke to him. Don't know much about him."
->opts
+[Finish]
{look and search and not BollywoodArmy: 
-> BollywoodArmy
}
-> END

=BollywoodArmy
// teasing/admonishing like he's been a naughty boy
I address the corpse.
Fingers: "Didn't know you were Army, Bollywood, you dark horse you."
// she thinks Fingers is making a joke.
Nose: "Oh, dark horse, because he's black", Nose said. 
If I could eye roll 360 degrees, I would've then.
Fingers: "No. First of all he's not black, he's Indian, I didn't know we was in the Army is what I mean."
Nose: Oh.
Nose figets, trying to break the awkward silence.
I revel in it.
Nose: "Was he a friend of yours?" She finally asks.
Fingers: "Stop now Nose."
-> END

=== Phones ===

=Unlock
// successfully got into a phone
{shuffle:
- Fingers: "Lovely jubbly."
- Fingers: "Here we go."
- Fingers: "Hello!"
}
-> END

=PhoneLocked
Fingers: "It's locked."
-> END

=UnlockFail
Fingers: "Hmmm, not you."
-> END

=== AvatarPhone ===
-(opts)
*[Check contacts]
Fingers: "Nothing stands out."
    -> opts
+{not Pin}[Check social media]
    {not LivingRoom.Lunch.look:
        Fingers: "Nothing interesting."
        ->opts
    }
    {LivingRoom.Lunch.look:
    #set KnowLunch = true
    I scroll through her feed.
    Fingers: "This guys got the same goofy T-shirt as our headless friend."
    Nose: "Let's see."
    Nose get's closer than I'd like.
    Nose: "That's Lunch, of course it is, should've known."
    I navigate to his profile.
    Fingers: "Lunch, real name Ben Hodgetts, what's his story?"
    Nose: "He's a gypo, everyone knows he's got a thing for Avatar, she plays on it they've done a few jobs together."
    Fingers: "What sort of jobs?"
    Nose: "Burglaries, shoplifting, habit paying stuff, all small time."
    Fingers: "And they thought they'd try moving up in the world. Why's he called Lunch?"
    Nose: "'Cause he's a salad."
    Fingers: "What?"
    Nose: "A lunch." 
    Nose: "A wet lettuce." 
    Nose: "A push over."
    I look back at the phone.
    }
    {LivingRoom.Lunch.search and LivingRoom.Lunch.look and not Pin: 
    -> Pin
    }
    
    -> opts
*[Check messages]
    Fingers: "Nothing of note."
    -> opts
*[Check recent calls]
    Fingers: "Nothing stands out."
    -> opts
+[Finish]
-->END

=Pin
Fingers: "This status, 'my darling daughter, I'm so proud blah blah blah, happy eighteenth'."
#set LunchPhoneUnlocked = true
Fingers: "15th July, this year, so born in '02. Worth a crack at Lunch's pin number."
->END

=== LunchPhone ===

=Unlock
{Unlock == 1: 
Fingers: "Pin worked. We're in."
}
-> Use


=Use
-(opts)
*[Check contacts]
    Fingers: "Avatar is here."
    -> opts
*[Check social media]
    Fingers: "Nothing we haven't already seen."
    -> opts
+{not callMoz}[Check messages]
Fingers: "A message to Mikey Murray, called him Moz."
Nose: "Yeah, Moz's name is Mike", Nose said.
    **(callMoz) [Call Moz]
        #wait 2.0
        #set RingMozPhone = true
        Fingers: "Where's your phone Moz?"
        -> END
    ++[Go back]
        -> opts
*[Check recent calls]
Fingers: "Nothing."
    -> opts
+[Finish]
->END

=== YoYoPhone ===
-(opts)
+[Check contacts]
    {not Garden.Bollywood.look:
    Fingers: "Moz is here."
    ->opts
    }
    {not checkYoYoMsgs: 
    Fingers: "Moz and Bollywood here."
    }
    {checkYoYoMsgs:
    Fingers: "There's a Brian, no surname."
    Nose: "You going to call him?" Nose asks.
    I smile with my mouth, but not my eyes.
    Fingers: "He might suspect something if Yoyo comes back from the dead."
    }
    -> opts
+[Check social media]
    {not checkYoYoMsgs: 
    Fingers: "Nothing stands out."
    -> opts
    }
    {checkYoYoMsgs and not Garden.Bollywood.search:
        Fingers: "I should check Paige and Bollywood's mutuals."
        Fingers: "But what's his name?"
        -> opts
    }
    {checkYoYoMsgs and Garden.Bollywood.search:
    Fingers: "Let's check mutual friends with Bollywood."
    -> LookForBrian
    }
    ->opts
*{not Garden.Bollywood.look}[Check messages]
    
    Fingers: "Get's messaged alot, nothing stands out."
    -> opts
*{Garden.Bollywood.look}[Check messages]
    Fingers: "A message to Bollywood 'don't make a move until those two fuck off and Brian get's here. I'll be back here until then.'"
    Things are becoming clear.
    Fingers: "Yoyo, Bollywood and Brian had a plan to rip Moz off."
    Nose: "What about them two?", Nose asks.
    Avatar and Lunch were a team.
    Fingers: "Reckon they had the same plan. Reckon they moved first too."
    Nose: "Whoa."
    Whoa indeed.
    Fingers: "I just need to pin down who this Brian is", I said.
    ~checkYoYoMsgs = true
    -> opts
    
*[Check recent calls]
    Fingers: "Call to Moz, check."
    -> opts
+[Finish]
->END

=== LookForBrian ===

I look through Paige's friends.
Fingers: "You know a lot of Singhs don't you?"
-(opts2)
    *[Chet Singh]
    *[David Singh]
    *[Jeff Singh]
        Fingers: "Here you are, Jeff. And one mutual friend called Brian."
        Social media is a bitch.
        Fingers: "Let's have good look at you."
        #set GameDone = true
        #toScene Lobby
        NoPlay:
    *[Parmjit Singh]
    *[Sochai Singh]
    *[Quit]
        ->END

// pick the wrong option        
-Fingers: "Nope."
-> opts2

=== MozPhone ===
-(opts)
*[Check contacts]
Fingers: "Lunch present and correct."
{Garden.Bollywood.look:
Fingers: "Bollywood too." 
}
    -> opts
*[Check social media]
    Fingers: "No socials, smart."
    -> opts
*[Check messages]
    Fingers: "The message from Lunch is here."
    -> opts
+ {not callYoyo} [Check recent calls]
    Fingers: "A late night call from Yoyo, you know a Yoyo?"
    Nose: "Heard the name, one of Moz's women I think."
    ** (callYoyo) [Call Yoyo]
        #set RingPaigePhone = true
        #wait 2.0
        Fingers: "Yoyo's phone is here."
        -> END
    ++ [Go back]
        -> opts
+[Finish]
->END

=== ArmyID ===
Fingers: "Bollywood's Army ID. Sergeant J Singh."
-> END

=== CallBoss ===
I call the boss.
Fingers: "I'm done here, one got away with some product", I said.
Boss: "You know who it is?" he asks.
Fingers: "I do."
Boss: "Good, tie up any loose ends, then bounce."
I make eye contact with Nose.
Fingers: "On it."
-> END
