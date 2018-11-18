USE [MarketData]
GO

/****** Object:  Table [dbo].[Table_Instruments]    Script Date: 2018/5/8 12:05:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Table_Instruments](
	[Ticker] [nchar](12) NOT NULL,
	[Name] [nchar](30) NULL,
	[Industory] [nchar](30) NULL,
	[Region] [nchar](30) NULL,
	[PE] [float] NULL,
	[PB] [float] NULL,
	[MarketValue] [float] NULL,
	[Currency] [nchar](8) NULL,
	[Margin] [float] NULL,
	[OrderFixedCost] [float] NULL,
	[OrderPercentCost] [float] NULL,
	[Memo] [text] NULL,
 CONSTRAINT [PK_Table_Instruments] PRIMARY KEY CLUSTERED 
(
	[Ticker] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Table_Instruments] ADD  CONSTRAINT [DF_Table_Instruments_Currency]  DEFAULT (N'RMB') FOR [Currency]
GO

/****** Object:  Table [dbo].[Table_TradePrice]    Script Date: 2018/5/8 12:07:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Table_TradePrice](
	[Time] [datetime] NOT NULL,
	[Ticker] [nchar](12) NOT NULL,
	[Open] [float] NULL,
	[Close] [float] NULL,
	[Low] [float] NULL,
	[High] [float] NULL,
	[Volume] [float] NULL,
	[Shares] [float] NULL,
 CONSTRAINT [PK_Table_TradePrice] PRIMARY KEY CLUSTERED 
(
	[Time] ASC,
	[Ticker] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


