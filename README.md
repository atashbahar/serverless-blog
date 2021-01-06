# Serverless Blog
A blog created using serverless technologies (AWS Lambda, DynamoDB and ...).

## Is this the most efficient way to run a blog?
No! There are many hosted services out there that you can easily have your blog up
and running in just few minutes and they certainly offer you way more options.

## How it works?
Here is a how all the pieces fit together:

![Architecture Diagram](https://res.atashbahar.com/ServerlessBlog.png)

This is what happens:
- Create your post in markdown format
- Upload it to `content` S3 bucket
- Processor Lambda converts it to HTML and stores it in DynamoDB
- Website reads from DynamoDB and shows the content
- There is also a `resources` S3 bucket behind CloudFront that you can use to
host your blog resources into (make sure they are publicly available)

## Can I use this for my own blog?
Of course! Feel free to use it as is or use it as a starting point for your own projects.

